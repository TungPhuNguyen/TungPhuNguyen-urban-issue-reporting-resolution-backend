using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.PostResolution.AutoCloseReports;

public sealed class AutoCloseResolvedReportsCommandHandler
    : IRequestHandler<AutoCloseResolvedReportsCommand, int>
{
    private const int AutoClosePeriodInDays = 7;
    private const int BatchSize = 200;

    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditLogService _auditLogService;

    public AutoCloseResolvedReportsCommandHandler(
        IApplicationDbContext dbContext,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    public async Task<int> Handle(
        AutoCloseResolvedReportsCommand request,
        CancellationToken cancellationToken)
    {
        var resolvedBefore =
            request.CurrentTime.AddDays(-AutoClosePeriodInDays);

        var reports = await _dbContext.Reports
            .Where(report =>
                report.Status == ReportStatus.Resolved
                && report.ResolvedAt.HasValue
                && report.ResolvedAt.Value <= resolvedBefore
                && !report.ComplaintSubmittedAt.HasValue)
            .OrderBy(report => report.ResolvedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var report in reports)
        {
            var oldStatus = report.Status;

            report.Status = ReportStatus.Closed;
            report.ClosedAt = request.CurrentTime;
            report.UpdatedAt = request.CurrentTime;

            _dbContext.StatusUpdates.Add(
                new StatusUpdate
                {
                    ReportId = report.Id,

                    // null biểu thị thao tác do hệ thống thực hiện.
                    UpdatedByUserId = null,

                    OldStatus = oldStatus,
                    NewStatus = ReportStatus.Closed,

                    Note =
                        "Hệ thống tự động đóng báo cáo "
                        + "sau 7 ngày không có khiếu nại.",

                    CreatedAt = request.CurrentTime
                });

            _auditLogService.Add(
                userId: null,
                action: AuditActions.ReportAutoClosed,
                entityType: AuditEntityTypes.Report,
                entityId: report.Id.ToString(),
                detail: new
                {
                    OldStatus = oldStatus,
                    NewStatus = report.Status,
                    report.ResolvedAt,
                    report.ClosedAt,
                    Reason = "Quá 7 ngày không có khiếu nại."
                });
        }

        if (reports.Count > 0)
        {
            /*
             * Report, StatusUpdate và AuditLog được lưu
             * chung trong một lần SaveChangesAsync.
             */
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return reports.Count;
    }
}
