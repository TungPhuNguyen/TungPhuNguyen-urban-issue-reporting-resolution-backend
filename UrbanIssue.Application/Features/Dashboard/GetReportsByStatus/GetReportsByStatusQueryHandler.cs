using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Dashboard.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Dashboard.GetReportsByStatus;

public sealed class GetReportsByStatusQueryHandler
    : IRequestHandler<
        GetReportsByStatusQuery,
        IReadOnlyList<ReportsByStatusResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetReportsByStatusQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReportsByStatusResult>>
        Handle(
            GetReportsByStatusQuery request,
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
                .GroupBy(report =>
                    report.Status)
                .Select(group => new
                {
                    Status = group.Key,
                    ReportCount = group.Count()
                })
                .ToListAsync(
                    cancellationToken);

        var totalReports =
            groupedItems.Sum(item =>
                item.ReportCount);

        /*
         * Luôn trả đủ tất cả trạng thái để frontend
         * không phải tự bổ sung trạng thái có số lượng 0.
         */
        return Enum.GetValues<ReportStatus>()
            .Select(status =>
            {
                var reportCount =
                    groupedItems
                        .Where(item =>
                            item.Status == status)
                        .Select(item =>
                            item.ReportCount)
                        .SingleOrDefault();

                var percentage =
                    totalReports == 0
                        ? 0
                        : Math.Round(
                            reportCount
                            * 100m
                            / totalReports,
                            2);

                return new ReportsByStatusResult(
                    Status: status,
                    ReportCount: reportCount,
                    Percentage: percentage);
            })
            .ToList();
    }
}
