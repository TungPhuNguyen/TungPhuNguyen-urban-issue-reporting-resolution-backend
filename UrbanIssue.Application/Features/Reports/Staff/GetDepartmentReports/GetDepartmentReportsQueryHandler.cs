using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.GetDepartmentReports;

public sealed class GetDepartmentReportsQueryHandler
    : IRequestHandler<
        GetDepartmentReportsQuery,
        PagedResult<StaffReportSummaryResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetDepartmentReportsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<StaffReportSummaryResult>> Handle(
        GetDepartmentReportsQuery request,
        CancellationToken cancellationToken)
    {
        var currentTime = DateTime.UtcNow;
        var staffId = _currentUserService.UserId;

        var staff = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == staffId)
            .Select(user => new
            {
                user.DepartmentId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (staff is null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy tài khoản nhân viên.");
        }

        if (!staff.DepartmentId.HasValue)
        {
            throw new ConflictException(
                "Tài khoản Staff chưa được gán phòng ban.");
        }

        var departmentId = staff.DepartmentId.Value;

        /*
         * Staff chỉ nhìn thấy phạm vi hợp lệ của mình:
         * - Report Assigned đang chờ trong Department.
         * - Report đã được chính Staff hiện tại tiếp nhận.
         *
         * Vì vậy filter IsOverdue không làm lộ Report của
         * Department khác hoặc Report do Staff khác phụ trách.
         */
        var query = _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.DepartmentId == departmentId
                && (
                    report.Status == ReportStatus.Assigned
                    || report.AssignedStaffId == staffId
                ));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(report =>
                report.ReportCode.Contains(search)
                || report.Title.Contains(search)
                || report.Description.Contains(search)
                || (
                    report.AddressText != null
                    && report.AddressText.Contains(search)
                )
                || report.Category.Name.Contains(search)
                || report.Area.Name.Contains(search));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(report =>
                report.Status == request.Status.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(report =>
                report.Priority == request.Priority.Value);
        }

        if (request.IsEscalated.HasValue)
        {
            query = query.Where(report =>
                report.IsEscalated
                    == request.IsEscalated.Value);
        }

        if (request.IsOverdue.HasValue)
        {
            if (request.IsOverdue.Value)
            {
                query = query.Where(report =>
                    (
                        report.Status == ReportStatus.Accepted
                        || report.Status == ReportStatus.InProgress
                    )
                    && report.DueAt.HasValue
                    && report.DueAt.Value < currentTime);
            }
            else
            {
                query = query.Where(report =>
                    !(
                        (
                            report.Status == ReportStatus.Accepted
                            || report.Status
                                == ReportStatus.InProgress
                        )
                        && report.DueAt.HasValue
                        && report.DueAt.Value < currentTime
                    ));
            }
        }

        var totalItems = await query.CountAsync(
            cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(
                totalItems / (double)request.PageSize);

        var rows = await query
            .OrderBy(report =>
                report.Status == ReportStatus.Assigned ? 0 : 1)
            .ThenBy(report =>
                (
                    report.Status == ReportStatus.Accepted
                    || report.Status == ReportStatus.InProgress
                )
                && report.DueAt.HasValue
                && report.DueAt.Value < currentTime
                    ? 0
                    : 1)
            .ThenBy(report => report.DueAt)
            .ThenByDescending(report => report.CreatedAt)
            .Skip(
                (request.PageNumber - 1)
                * request.PageSize)
            .Take(request.PageSize)
            .Select(report => new
            {
                report.Id,
                report.ReportCode,
                report.Title,
                report.CategoryId,
                CategoryName = report.Category.Name,
                report.AreaId,
                AreaName = report.Area.Name,
                report.Description,
                report.OtherCategoryText,
                report.AddressText,
                report.Priority,
                report.Status,
                report.AssignedStaffId,
                AssignedStaffName = report.AssignedStaff == null
                    ? null
                    : report.AssignedStaff.FullName,
                report.RequiresManualAssignment,
                UpvoteCount = report.Upvotes.Count(),
                ThumbnailUrl = report.Images
                    .OrderBy(image => image.Id)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault(),
                report.CreatedAt,
                report.DueAt,
                report.IsEscalated,
                report.EscalatedAt
            })
            .ToListAsync(cancellationToken);

        /*
         * Tính thời lượng quá hạn sau khi đã lấy đúng một trang.
         * Không dùng hàm riêng của SQL Server trong Application.
         */
        var items = rows
            .Select(row =>
            {
                var isOverdue =
                    (
                        row.Status == ReportStatus.Accepted
                        || row.Status == ReportStatus.InProgress
                    )
                    && row.DueAt.HasValue
                    && row.DueAt.Value < currentTime;

                var overdueHours = isOverdue
                    ? Math.Round(
                        (
                            currentTime
                            - row.DueAt!.Value
                        ).TotalHours,
                        2)
                    : (double?)null;

                return new StaffReportSummaryResult(
                    row.Id,
                    row.ReportCode,
                    row.Title,
                    row.CategoryId,
                    row.CategoryName,
                    row.AreaId,
                    row.AreaName,
                    row.Description,
                    row.OtherCategoryText,
                    row.AddressText,
                    row.Priority,
                    row.Status,
                    row.AssignedStaffId,
                    row.AssignedStaffName,
                    row.RequiresManualAssignment,
                    row.UpvoteCount,
                    row.ThumbnailUrl,
                    row.CreatedAt,
                    row.DueAt,
                    isOverdue,
                    overdueHours,
                    row.IsEscalated,
                    row.EscalatedAt);
            })
            .ToList();

        return new PagedResult<StaffReportSummaryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
