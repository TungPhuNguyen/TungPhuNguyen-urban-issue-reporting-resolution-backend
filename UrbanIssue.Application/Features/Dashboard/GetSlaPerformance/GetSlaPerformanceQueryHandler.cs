using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Dashboard.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Dashboard.GetSlaPerformance;

public sealed class GetSlaPerformanceQueryHandler
    : IRequestHandler<
        GetSlaPerformanceQuery,
        SlaPerformanceResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSlaPerformanceQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SlaPerformanceResult> Handle(
        GetSlaPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        var range =
            DashboardDateRangeResolver.Resolve(
                request.FromDate,
                request.ToDate);

        var currentTime = DateTime.UtcNow;

        /*
         * SLA Performance lọc theo thời điểm SLA bắt đầu,
         * không lọc theo CreatedAt của Report.
         */
        var slaQuery =
            _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.SLAStartedAt.HasValue
                    && report.DueAt.HasValue
                    && report.SLAStartedAt.Value
                        >= range.FromUtc
                    && report.SLAStartedAt.Value
                        < range.ToExclusiveUtc);

        var aggregate =
            await slaQuery
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    SlaTrackedReports =
                        group.Count(),

                    CompletedReports =
                        group.Count(report =>
                            report.ResolvedAt.HasValue),

                    CompletedOnTimeReports =
                        group.Count(report =>
                            report.ResolvedAt.HasValue
                            && report.ResolvedAt.Value
                                <= report.DueAt!.Value),

                    CompletedLateReports =
                        group.Count(report =>
                            report.ResolvedAt.HasValue
                            && report.ResolvedAt.Value
                                > report.DueAt!.Value),

                    ActiveOverdueReports =
                        group.Count(report =>
                            (
                                report.Status
                                    == ReportStatus.Accepted
                                || report.Status
                                    == ReportStatus.InProgress
                            )
                            && report.DueAt!.Value
                                < currentTime),

                    EscalatedReports =
                        group.Count(report =>
                            report.IsEscalated)
                })
                .SingleOrDefaultAsync(
                    cancellationToken);

        /*
         * Chỉ lấy hai trường thời gian cần thiết,
         * sau đó tính khoảng thời gian trong bộ nhớ.
         */
        var completedDurations =
            await slaQuery
                .Where(report =>
                    report.ResolvedAt.HasValue)
                .Select(report => new
                {
                    SlaStartedAt =
                        report.SLAStartedAt!.Value,

                    ResolvedAt =
                        report.ResolvedAt!.Value
                })
                .ToListAsync(
                    cancellationToken);

        double? averageHandlingHours = null;

        if (completedDurations.Count > 0)
        {
            averageHandlingHours =
                Math.Round(
                    completedDurations.Average(item =>
                        (
                            item.ResolvedAt
                            - item.SlaStartedAt
                        ).TotalHours),
                    2);
        }

        if (aggregate is null)
        {
            return new SlaPerformanceResult(
                FromDate: range.FromDate,
                ToDate: range.ToDate,
                SlaTrackedReports: 0,
                CompletedReports: 0,
                CompletedOnTimeReports: 0,
                CompletedLateReports: 0,
                ActiveOverdueReports: 0,
                EscalatedReports: 0,
                ComplianceRate: 0,
                AverageHandlingHours: null);
        }

        var complianceRate =
            aggregate.CompletedReports == 0
                ? 0
                : Math.Round(
                    aggregate.CompletedOnTimeReports
                    * 100m
                    / aggregate.CompletedReports,
                    2);

        return new SlaPerformanceResult(
            FromDate: range.FromDate,
            ToDate: range.ToDate,

            SlaTrackedReports:
                aggregate.SlaTrackedReports,

            CompletedReports:
                aggregate.CompletedReports,

            CompletedOnTimeReports:
                aggregate.CompletedOnTimeReports,

            CompletedLateReports:
                aggregate.CompletedLateReports,

            ActiveOverdueReports:
                aggregate.ActiveOverdueReports,

            EscalatedReports:
                aggregate.EscalatedReports,

            ComplianceRate:
                complianceRate,

            AverageHandlingHours:
                averageHandlingHours);
    }
}
