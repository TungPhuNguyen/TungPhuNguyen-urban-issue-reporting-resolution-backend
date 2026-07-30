using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetReportsByCategory;

public sealed class GetReportsByCategoryQueryHandler
    : IRequestHandler<
        GetReportsByCategoryQuery,
        IReadOnlyList<ReportsByCategoryResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetReportsByCategoryQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReportsByCategoryResult>>
        Handle(
            GetReportsByCategoryQuery request,
            CancellationToken cancellationToken)
    {
        var range =
            DashboardDateRangeResolver.Resolve(
                request.FromDate,
                request.ToDate);

        var reportQuery =
            _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.CreatedAt
                        >= range.FromUtc
                    && report.CreatedAt
                        < range.ToExclusiveUtc);

        var groupedItems =
            await reportQuery
                .GroupBy(report => new
                {
                    report.CategoryId,
                    CategoryName =
                        report.Category.Name
                })
                .Select(group => new
                {
                    group.Key.CategoryId,
                    group.Key.CategoryName,
                    ReportCount = group.Count()
                })
                .OrderByDescending(item =>
                    item.ReportCount)
                .ThenBy(item =>
                    item.CategoryName)
                .ToListAsync(
                    cancellationToken);

        var completedDurations =
            await reportQuery
                .Where(report =>
                    report.SLAStartedAt.HasValue
                    && report.ResolvedAt.HasValue
                    && report.ResolvedAt.Value
                        >= report.SLAStartedAt.Value)
                .Select(report => new
                {
                    report.CategoryId,
                    SLAStartedAt =
                        report.SLAStartedAt!.Value,
                    ResolvedAt =
                        report.ResolvedAt!.Value
                })
                .ToListAsync(
                    cancellationToken);

        var averageHandlingHoursByCategory =
            completedDurations
                .GroupBy(item => item.CategoryId)
                .ToDictionary(
                    group => group.Key,
                    group => Math.Round(
                        group.Average(item =>
                            (
                                item.ResolvedAt
                                - item.SLAStartedAt
                            ).TotalHours),
                        2));

        var totalReports =
            groupedItems.Sum(item =>
                item.ReportCount);

        return groupedItems
            .Select(item =>
                new ReportsByCategoryResult(
                    CategoryId:
                        item.CategoryId,

                    CategoryName:
                        item.CategoryName,

                    ReportCount:
                        item.ReportCount,

                    Percentage:
                        totalReports == 0
                            ? 0
                            : Math.Round(
                                item.ReportCount
                                * 100m
                                / totalReports,
                                2),

                    AverageHandlingHours:
                        averageHandlingHoursByCategory
                            .TryGetValue(
                                item.CategoryId,
                                out var averageHours)
                            ? averageHours
                            : null))
            .ToList();
    }
}
