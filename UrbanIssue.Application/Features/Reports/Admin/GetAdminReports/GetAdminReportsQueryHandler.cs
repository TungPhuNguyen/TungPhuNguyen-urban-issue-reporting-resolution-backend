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
        var query = _dbContext.Reports
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(report =>
                report.Description.Contains(search)
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

        if (request.RequiresManualAssignment.HasValue)
        {
            query = query.Where(report =>
                report.RequiresManualAssignment
                    == request.RequiresManualAssignment.Value);
        }

        if (request.HasComplaint.HasValue)
        {
            if (request.HasComplaint.Value)
            {
                query = query.Where(report =>
                    report.ComplaintSubmittedAt.HasValue);
            }
            else
            {
                query = query.Where(report =>
                    !report.ComplaintSubmittedAt.HasValue);
            }
        }

        var totalItems = await query.CountAsync(
            cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(
                totalItems / (double)request.PageSize);

        var items = await query
            .OrderByDescending(report =>
                report.RequiresManualAssignment)
            .ThenBy(report =>
                report.Status == ReportStatus.New ? 0 : 1)
            .ThenByDescending(report =>
                report.ComplaintSubmittedAt.HasValue)
            .ThenByDescending(report =>
                report.CreatedAt)
            .Skip(
                (request.PageNumber - 1)
                * request.PageSize)
            .Take(request.PageSize)
            .Select(report =>
                new AdminReportSummaryResult(
                    report.Id,

                    report.Citizen.FullName,

                    report.CategoryId,
                    report.Category.Name,

                    report.AreaId,
                    report.Area.Name,

                    report.DepartmentId,
                    report.Department == null
                        ? null
                        : report.Department.Name,

                    report.AssignedStaffId,
                    report.AssignedStaff == null
                        ? null
                        : report.AssignedStaff.FullName,

                    report.Description,
                    report.Priority,
                    report.Status,
                    report.RequiresManualAssignment,

                    report.ComplaintSubmittedAt.HasValue,

                    report.Upvotes.Count(),

                    report.Images
                        .OrderBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault(),

                    report.CreatedAt,
                    report.DueAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminReportSummaryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
