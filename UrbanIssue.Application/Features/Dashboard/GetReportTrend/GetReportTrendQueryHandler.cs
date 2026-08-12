using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetReportTrend;

public sealed class GetReportTrendQueryHandler
    : IRequestHandler<
        GetReportTrendQuery,
        ReportTrendResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetReportTrendQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportTrendResult> Handle(
        GetReportTrendQuery request,
        CancellationToken cancellationToken)
    {
        var range =
            DashboardDateRangeResolver.Resolve(
                request.FromDate,
                request.ToDate);

        var createdItems =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.CreatedAt
                        >= range.FromUtc
                    && report.CreatedAt
                        < range.ToExclusiveUtc)
                .GroupBy(report =>
                    report.CreatedAt.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(
                    cancellationToken);

        var resolvedItems =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.ResolvedAt.HasValue
                    && report.ResolvedAt.Value
                        >= range.FromUtc
                    && report.ResolvedAt.Value
                        < range.ToExclusiveUtc)
                .GroupBy(report =>
                    report.ResolvedAt!.Value.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(
                    cancellationToken);

        var closedItems =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.ClosedAt.HasValue
                    && report.ClosedAt.Value
                        >= range.FromUtc
                    && report.ClosedAt.Value
                        < range.ToExclusiveUtc)
                .GroupBy(report =>
                    report.ClosedAt!.Value.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(
                    cancellationToken);

        var createdLookup =
            createdItems.ToDictionary(
                item =>
                    DateOnly.FromDateTime(item.Date),
                item => item.Count);

        var resolvedLookup =
            resolvedItems.ToDictionary(
                item =>
                    DateOnly.FromDateTime(item.Date),
                item => item.Count);

        var closedLookup =
            closedItems.ToDictionary(
                item =>
                    DateOnly.FromDateTime(item.Date),
                item => item.Count);

        var trendItems =
            new List<ReportTrendItemResult>();

        for (
            var date = range.FromDate;
            date <= range.ToDate;
            date = date.AddDays(1))
        {
            trendItems.Add(
                new ReportTrendItemResult(
                    Date: date,

                    CreatedCount:
                        createdLookup.GetValueOrDefault(date),

                    ResolvedCount:
                        resolvedLookup.GetValueOrDefault(date),

                    ClosedCount:
                        closedLookup.GetValueOrDefault(date)));
        }

        return new ReportTrendResult(
            FromDate: range.FromDate,
            ToDate: range.ToDate,
            Items: trendItems);
    }
}
