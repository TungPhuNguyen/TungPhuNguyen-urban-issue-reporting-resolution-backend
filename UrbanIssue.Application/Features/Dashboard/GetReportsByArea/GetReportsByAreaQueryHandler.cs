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

        var groupedItems =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.CreatedAt
                        >= range.FromUtc
                    && report.CreatedAt
                        < range.ToExclusiveUtc)
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
                                2)))
            .ToList();
    }
}
