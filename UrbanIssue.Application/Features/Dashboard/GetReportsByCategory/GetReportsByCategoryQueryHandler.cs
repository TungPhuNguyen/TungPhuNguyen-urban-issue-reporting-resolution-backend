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
                                2)))
            .ToList();
    }
}
