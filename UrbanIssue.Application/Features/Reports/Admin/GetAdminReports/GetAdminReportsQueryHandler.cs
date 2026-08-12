using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Admin.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Admin.GetAdminReports;

public sealed class GetAdminReportsQueryHandler
    : IRequestHandler<
        GetAdminReportsQuery,
        PagedResult<AdminReportSummaryResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdminReportsQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminReportSummaryResult>> Handle(
        GetAdminReportsQuery request,
        CancellationToken cancellationToken)
    {
        var currentTime = DateTime.UtcNow;

        var query = _dbContext.Reports
            .AsNoTracking()
            .AsQueryable();

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
                || report.Citizen.FullName.Contains(search)
                || report.Citizen.Email.Contains(search)
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

        if (request.CategoryId.HasValue)
        {
            query = query.Where(report =>
                report.CategoryId == request.CategoryId.Value);
        }

        if (request.AreaId.HasValue)
        {
            query = query.Where(report =>
                report.AreaId == request.AreaId.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(report =>
                report.DepartmentId == request.DepartmentId.Value);
        }

        if (request.StaffId.HasValue)
        {
            query = query.Where(report =>
                report.AssignedStaffId == request.StaffId.Value);
        }

        if (request.RequiresManualAssignment.HasValue)
        {
            query = query.Where(report =>
                report.RequiresManualAssignment
                    == request.RequiresManualAssignment.Value);
        }

        if (request.HasComplaint.HasValue)
        {
            query = request.HasComplaint.Value
                ? query.Where(report =>
                    report.Complaints.Any(complaint =>
                        complaint.Status == ComplaintStatus.Pending))
                : query.Where(report =>
                    !report.Complaints.Any(complaint =>
                        complaint.Status == ComplaintStatus.Pending));
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
            .OrderByDescending(report =>
                report.RequiresManualAssignment)
            .ThenBy(report =>
                report.Status == ReportStatus.New ? 0 : 1)
                .ThenByDescending(report =>
                    report.Complaints.Any(complaint =>
                        complaint.Status == ComplaintStatus.Pending))
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
                CitizenName = report.Citizen.FullName,
                report.CategoryId,
                CategoryName = report.Category.Name,
                report.AreaId,
                AreaName = report.Area.Name,
                report.DepartmentId,
                DepartmentName = report.Department == null
                    ? null
                    : report.Department.Name,
                report.AssignedStaffId,
                AssignedStaffName = report.AssignedStaff == null
                    ? null
                    : report.AssignedStaff.FullName,
                report.Description,
                report.OtherCategoryText,
                report.Priority,
                report.Status,
                report.RequiresManualAssignment,
                HasComplaint =
                    report.Complaints.Any(complaint =>
                        complaint.Status == ComplaintStatus.Pending),
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
         * Cách này giữ Application độc lập với provider SQL Server
         * và tránh phụ thuộc EF.Functions.DateDiff*.
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

                return new AdminReportSummaryResult(
                    row.Id,
                    row.ReportCode,
                    row.Title,
                    row.CitizenName,
                    row.CategoryId,
                    row.CategoryName,
                    row.AreaId,
                    row.AreaName,
                    row.DepartmentId,
                    row.DepartmentName,
                    row.AssignedStaffId,
                    row.AssignedStaffName,
                    row.Description,
                    row.OtherCategoryText,
                    row.Priority,
                    row.Status,
                    row.RequiresManualAssignment,
                    row.HasComplaint,
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

        return new PagedResult<AdminReportSummaryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
