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
        var staffId = _currentUserService.UserId;

        var staff = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == staffId)
            .Select(x => new
            {
                x.DepartmentId
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
         * Staff nhìn thấy:
         * - Report Assigned đang nằm trong hàng đợi phòng ban.
         * - Report đã được chính Staff đó tiếp nhận.
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
                report.Description.Contains(search)
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

        var totalItems = await query.CountAsync(
            cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(
                totalItems / (double)request.PageSize);

        var items = await query
            .OrderBy(report =>
                report.Status == ReportStatus.Assigned ? 0 : 1)
            .ThenBy(report => report.DueAt)
            .ThenByDescending(report => report.CreatedAt)
            .Skip(
                (request.PageNumber - 1)
                * request.PageSize)
            .Take(request.PageSize)
            .Select(report =>
                new StaffReportSummaryResult(
                    report.Id,

                    report.CategoryId,
                    report.Category.Name,

                    report.AreaId,
                    report.Area.Name,

                    report.Description,
                    report.AddressText,

                    report.Priority,
                    report.Status,

                    report.AssignedStaffId,
                    report.AssignedStaff == null
                        ? null
                        : report.AssignedStaff.FullName,

                    report.RequiresManualAssignment,

                    report.Upvotes.Count(),

                    report.Images
                        .OrderBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault(),

                    report.CreatedAt,
                    report.DueAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<StaffReportSummaryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
