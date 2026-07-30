using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetReportsByArea;

public sealed class GetReportsByAreaQueryHandler
    : IRequestHandler<
        GetReportsByAreaQuery,
        IReadOnlyList<ReportsByAreaResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetReportsByAreaQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReportsByAreaResult>>
        Handle(
            GetReportsByAreaQuery request,
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
                    report.AreaId,
                    AreaName =
                        report.Area.Name
                })
                .Select(group => new
                {
                    group.Key.AreaId,
                    group.Key.AreaName,
                    ReportCount = group.Count()
                })
                .OrderByDescending(item =>
                    item.ReportCount)
                .ThenBy(item =>
                    item.AreaName)
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
                    report.AreaId,
                    SLAStartedAt =
                        report.SLAStartedAt!.Value,
                    ResolvedAt =
                        report.ResolvedAt!.Value
                })
                .ToListAsync(
                    cancellationToken);

        var averageHandlingHoursByArea =
            completedDurations
                .GroupBy(item => item.AreaId)
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
                new ReportsByAreaResult(
                    AreaId:
                        item.AreaId,

                    AreaName:
                        item.AreaName,

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
                        averageHandlingHoursByArea
                            .TryGetValue(
                                item.AreaId,
                                out var averageHours)
                            ? averageHours
                            : null))
            .ToList();
    }
}
