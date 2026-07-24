using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Dashboard.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Dashboard.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<
        GetDashboardSummaryQuery,
        DashboardSummaryResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryResult> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var range =
            DashboardDateRangeResolver.Resolve(
                request.FromDate,
                request.ToDate);

        var currentTime = DateTime.UtcNow;

        var query = _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.CreatedAt >= range.FromUtc
                && report.CreatedAt
                    < range.ToExclusiveUtc);

        var aggregate = await query
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalReports =
                    group.Count(),

                NewReports =
                    group.Count(report =>
                        report.Status
                            == ReportStatus.New),

                AssignedReports =
                    group.Count(report =>
                        report.Status
                            == ReportStatus.Assigned),

                AcceptedReports =
                    group.Count(report =>
                        report.Status
                            == ReportStatus.Accepted),

                InProgressReports =
                    group.Count(report =>
                        report.Status
                            == ReportStatus.InProgress),

                ResolvedReports =
                    group.Count(report =>
                        report.Status
                            == ReportStatus.Resolved),

                ClosedReports =
                    group.Count(report =>
                        report.Status
                            == ReportStatus.Closed),

                RejectedReports =
                    group.Count(report =>
                        report.Status
                            == ReportStatus.Rejected),

                RequiresManualAssignmentReports =
                    group.Count(report =>
                        report.RequiresManualAssignment),

                PendingComplaintReports =
                    group.Count(report =>
                        report.ComplaintSubmittedAt.HasValue
                        && report.Status
                            == ReportStatus.Resolved),

                ActiveOverdueReports =
                    group.Count(report =>
                        (
                            report.Status
                                == ReportStatus.Accepted
                            || report.Status
                                == ReportStatus.InProgress
                        )
                        && report.DueAt.HasValue
                        && report.DueAt.Value
                            < currentTime),

                EscalatedReports =
                    group.Count(report =>
                        report.IsEscalated)
            })
            .SingleOrDefaultAsync(
                cancellationToken);

        if (aggregate is null)
        {
            return new DashboardSummaryResult(
                FromDate: range.FromDate,
                ToDate: range.ToDate,
                TotalReports: 0,
                NewReports: 0,
                AssignedReports: 0,
                AcceptedReports: 0,
                InProgressReports: 0,
                ResolvedReports: 0,
                ClosedReports: 0,
                RejectedReports: 0,
                RequiresManualAssignmentReports: 0,
                PendingComplaintReports: 0,
                ActiveOverdueReports: 0,
                EscalatedReports: 0,
                ResolutionRate: 0);
        }

        var completedReports =
            aggregate.ResolvedReports
            + aggregate.ClosedReports;

        var eligibleReports =
            aggregate.TotalReports
            - aggregate.RejectedReports;

        var resolutionRate =
            eligibleReports == 0
                ? 0
                : Math.Round(
                    completedReports
                    * 100m
                    / eligibleReports,
                    2);

        return new DashboardSummaryResult(
            FromDate: range.FromDate,
            ToDate: range.ToDate,

            TotalReports:
                aggregate.TotalReports,

            NewReports:
                aggregate.NewReports,

            AssignedReports:
                aggregate.AssignedReports,

            AcceptedReports:
                aggregate.AcceptedReports,

            InProgressReports:
                aggregate.InProgressReports,

            ResolvedReports:
                aggregate.ResolvedReports,

            ClosedReports:
                aggregate.ClosedReports,

            RejectedReports:
                aggregate.RejectedReports,

            RequiresManualAssignmentReports:
                aggregate.RequiresManualAssignmentReports,

            PendingComplaintReports:
                aggregate.PendingComplaintReports,

            ActiveOverdueReports:
                aggregate.ActiveOverdueReports,

            EscalatedReports:
                aggregate.EscalatedReports,

            ResolutionRate:
                resolutionRate);
    }
}
