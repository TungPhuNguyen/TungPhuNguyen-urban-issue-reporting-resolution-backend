using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.SlaMonitoring;

public sealed class ProcessSlaMonitoringCommandHandler
    : IRequestHandler<
        ProcessSlaMonitoringCommand,
        ProcessSlaMonitoringResult>
{
    private const double WarningThresholdRatio = 0.8d;
    private const double EscalationThresholdRatio = 1.5d;
    private const int BatchSize = 200;

    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;

    public ProcessSlaMonitoringCommandHandler(
        IApplicationDbContext dbContext,
        INotificationService notificationService,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
    }

    public async Task<ProcessSlaMonitoringResult> Handle(
        ProcessSlaMonitoringCommand request,
        CancellationToken cancellationToken)
    {
        var currentTime = request.CurrentTime;

        /*
         * Ba mốc độc lập:
         *
         * Warning    = 80% thời gian SLA
         * Breach     = DueAt
         * Escalation = 150% thời gian SLA
         */
        var reports = await _dbContext.Reports
            .Where(report =>
                (
                    report.Status == ReportStatus.Accepted
                    || report.Status == ReportStatus.InProgress
                )
                && report.SLAStartedAt.HasValue
                && report.AppliedSLAHours.HasValue
                && report.AppliedSLAHours.Value > 0
                && report.DueAt.HasValue
                && (
                    (
                        !report.SLAWarningSentAt.HasValue
                        && currentTime < report.DueAt.Value
                        && report.SLAStartedAt.Value.AddHours(
                            report.AppliedSLAHours.Value
                            * WarningThresholdRatio)
                            <= currentTime
                    )
                    || (
                        !report.SLABreachedNotifiedAt.HasValue
                        && report.DueAt.Value <= currentTime
                    )
                    || (
                        !report.IsEscalated
                        && report.SLAStartedAt.Value.AddHours(
                            report.AppliedSLAHours.Value
                            * EscalationThresholdRatio)
                            <= currentTime
                    )
                ))
            .OrderBy(report => report.DueAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (reports.Count == 0)
        {
            return new ProcessSlaMonitoringResult(
                WarningReportCount: 0,
                BreachedReportCount: 0,
                EscalatedReportCount: 0,
                CreatedNotificationCount: 0);
        }

        var adminIds = await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.IsActive
                && user.Role.Name == "Admin")
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        var warningReportCount = 0;
        var breachedReportCount = 0;
        var escalatedReportCount = 0;
        var createdNotificationCount = 0;

        foreach (var report in reports)
        {
            var slaStartedAt =
                report.SLAStartedAt!.Value;

            var appliedSlaHours =
                report.AppliedSLAHours!.Value;

            var dueAt =
                report.DueAt!.Value;

            var warningAt =
                slaStartedAt.AddHours(
                    appliedSlaHours
                    * WarningThresholdRatio);

            var escalationAt =
                slaStartedAt.AddHours(
                    appliedSlaHours
                    * EscalationThresholdRatio);

            if (!report.SLAWarningSentAt.HasValue
                && currentTime >= warningAt
                && currentTime < dueAt)
            {
                report.SLAWarningSentAt = currentTime;
                report.UpdatedAt = currentTime;
                warningReportCount++;

                var recipientIds =
                    new HashSet<Guid>(adminIds);

                if (report.AssignedStaffId.HasValue)
                {
                    recipientIds.Add(
                        report.AssignedStaffId.Value);
                }

                _notificationService.AddMany(
                    userIds: recipientIds,
                    reportId: report.Id,
                    type: NotificationType.SLAWarning,
                    title: "Báo cáo sắp hết hạn SLA",
                    message:
                        $"Báo cáo {report.Id} sắp hết hạn "
                        + $"SLA lúc "
                        + $"{dueAt:dd/MM/yyyy HH:mm} UTC.",
                    createdAt: currentTime);

                createdNotificationCount +=
                    recipientIds.Count;

                _auditLogService.Add(
                    userId: null,
                    action: AuditActions.SlaWarningSent,
                    entityType: AuditEntityTypes.Report,
                    entityId: report.Id.ToString(),
                    detail: new
                    {
                        report.Status,
                        report.SLAStartedAt,
                        report.AppliedSLAHours,
                        report.DueAt,
                        report.SLAWarningSentAt,
                        WarningAt = warningAt,
                        RecipientCount =
                            recipientIds.Count
                    });
            }

            /*
             * Breach chỉ đánh dấu quá hạn.
             * Không Escalate ngay tại DueAt.
             */
            if (!report.SLABreachedNotifiedAt.HasValue
                && currentTime >= dueAt)
            {
                report.SLABreachedNotifiedAt =
                    currentTime;

                report.UpdatedAt =
                    currentTime;

                breachedReportCount++;

                if (report.AssignedStaffId.HasValue)
                {
                    _notificationService.Add(
                        userId:
                            report.AssignedStaffId.Value,
                        reportId:
                            report.Id,
                        type:
                            NotificationType.SLABreached,
                        title:
                            "Báo cáo đã quá hạn SLA",
                        message:
                            $"Báo cáo {report.Id} "
                            + "đã quá thời hạn xử lý.",
                        createdAt:
                            currentTime);

                    createdNotificationCount++;
                }

                _notificationService.Add(
                    userId: report.CitizenId,
                    reportId: report.Id,
                    type: NotificationType.SLABreached,
                    title: "Báo cáo đang xử lý chậm",
                    message:
                        "Báo cáo của bạn đã quá thời hạn "
                        + "xử lý dự kiến.",
                    createdAt: currentTime);

                createdNotificationCount++;

                _dbContext.StatusUpdates.Add(
                    new StatusUpdate
                    {
                        ReportId = report.Id,
                        UpdatedByUserId = null,
                        OldStatus = report.Status,
                        NewStatus = report.Status,
                        Note =
                            "Hệ thống phát hiện báo cáo "
                            + "đã quá hạn SLA.",
                        CreatedAt = currentTime
                    });

                _auditLogService.Add(
                    userId: null,
                    action: AuditActions.SlaBreached,
                    entityType: AuditEntityTypes.Report,
                    entityId: report.Id.ToString(),
                    detail: new
                    {
                        report.Status,
                        report.SLAStartedAt,
                        report.AppliedSLAHours,
                        report.DueAt,
                        report.SLABreachedNotifiedAt,
                        AssignedStaffNotified =
                            report.AssignedStaffId.HasValue,
                        CitizenNotified = true
                    });
            }

            /*
             * Chỉ Escalate khi đã sử dụng ít nhất 150%
             * tổng thời gian SLA.
             */
            if (!report.IsEscalated
                && currentTime >= escalationAt)
            {
                report.IsEscalated = true;
                report.EscalatedAt = currentTime;
                report.UpdatedAt = currentTime;
                escalatedReportCount++;

                _notificationService.AddMany(
                    userIds: adminIds,
                    reportId: report.Id,
                    type: NotificationType.Escalated,
                    title: "Báo cáo SLA bị Escalated",
                    message:
                        $"Báo cáo {report.Id} đã vượt "
                        + "150% thời gian SLA và cần "
                        + "Admin can thiệp.",
                    createdAt: currentTime);

                createdNotificationCount +=
                    adminIds.Count;

                _dbContext.StatusUpdates.Add(
                    new StatusUpdate
                    {
                        ReportId = report.Id,
                        UpdatedByUserId = null,
                        OldStatus = report.Status,
                        NewStatus = report.Status,
                        Note =
                            "Hệ thống Escalate báo cáo "
                            + "sau khi vượt 150% thời gian SLA.",
                        CreatedAt = currentTime
                    });

                _auditLogService.Add(
                    userId: null,
                    action: AuditActions.SlaEscalated,
                    entityType: AuditEntityTypes.Report,
                    entityId: report.Id.ToString(),
                    detail: new
                    {
                        report.Status,
                        report.SLAStartedAt,
                        report.AppliedSLAHours,
                        report.DueAt,
                        EscalationAt = escalationAt,
                        report.IsEscalated,
                        report.EscalatedAt,
                        AdminRecipientCount =
                            adminIds.Count
                    });
            }
        }

        if (warningReportCount > 0
            || breachedReportCount > 0
            || escalatedReportCount > 0)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return new ProcessSlaMonitoringResult(
            WarningReportCount:
                warningReportCount,
            BreachedReportCount:
                breachedReportCount,
            EscalatedReportCount:
                escalatedReportCount,
            CreatedNotificationCount:
                createdNotificationCount);
    }
}
