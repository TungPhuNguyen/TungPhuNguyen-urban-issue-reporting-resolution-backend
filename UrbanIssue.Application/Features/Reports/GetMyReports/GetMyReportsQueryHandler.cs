using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.GetMyReports;

public sealed class GetMyReportsQueryHandler
    : IRequestHandler<
        GetMyReportsQuery,
        PagedResult<CitizenReportSummaryResult>>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public GetMyReportsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<
        PagedResult<CitizenReportSummaryResult>>
        Handle(
            GetMyReportsQuery request,
            CancellationToken cancellationToken)
    {
        var citizenId =
            _currentUserService.UserId;

        var reportsQuery =
            _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.CitizenId == citizenId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch =
                request.Search.Trim();

            reportsQuery =
                reportsQuery.Where(report =>
                    report.Description.Contains(
                        normalizedSearch)

                    || (
                        report.AddressText != null
                        && report.AddressText.Contains(
                            normalizedSearch))

                    || report.Category.Name.Contains(
                        normalizedSearch)

                    || report.Area.Name.Contains(
                        normalizedSearch));
        }

        if (request.Status.HasValue)
        {
            reportsQuery =
                reportsQuery.Where(report =>
                    report.Status
                        == request.Status.Value);
        }

        var totalItems =
            await reportsQuery.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await reportsQuery
                .OrderByDescending(report =>
                    report.CreatedAt)
                .ThenByDescending(report =>
                    report.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(report =>
                    new CitizenReportSummaryResult(
                        report.Id,

                        report.CategoryId,
                        report.Category.Name,

                        report.AreaId,
                        report.Area.Name,

                        report.DepartmentId,
                        report.Department == null
                            ? null
                            : report.Department.Name,

                        report.Description,
                        report.AddressText,

                        report.Priority,
                        report.Status,

                        report.RequiresManualAssignment,

                        report.Upvotes.Count(),

                        report.Images
                            .OrderBy(image => image.Id)
                            .Select(image =>
                                image.ImageUrl)
                            .FirstOrDefault(),

                        report.CreatedAt,
                        report.UpdatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<CitizenReportSummaryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
