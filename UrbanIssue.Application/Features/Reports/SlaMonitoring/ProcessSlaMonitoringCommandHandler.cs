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
         * Chỉ lấy các Report thực sự cần xử lý:
         *
         * - Accepted hoặc InProgress
         * - Có đầy đủ SLA snapshot
         * - Đã quá hạn và chưa gửi breach
         *   hoặc
         * - Đã đạt ngưỡng cảnh báo 80% và chưa gửi warning
         */
        var reports = await _dbContext.Reports
            .Where(report =>
                (
                    report.Status == ReportStatus.Accepted
                    || report.Status == ReportStatus.InProgress
                )
                && report.SLAStartedAt.HasValue
                && report.AppliedSLAHours.HasValue
                && report.DueAt.HasValue
                && (
                    (
                        !report.SLABreachedNotifiedAt.HasValue
                        && report.DueAt.Value <= currentTime
                    )
                    || (
                        !report.SLAWarningSentAt.HasValue
                        && currentTime < report.DueAt.Value
                        && report.SLAStartedAt.Value.AddHours(
                            report.AppliedSLAHours.Value
                            * WarningThresholdRatio) <= currentTime
                    )
                ))
            /*
             * Ưu tiên xử lý Report đã quá hạn trước.
             */
            .OrderBy(report =>
                report.DueAt > currentTime)
            .ThenBy(report => report.DueAt)
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

            /*
             * Cảnh báo khi:
             *
             * - Đã sử dụng ít nhất 80% SLA
             * - Chưa quá hạn
             * - Chưa từng gửi cảnh báo
             */
            if (!report.SLAWarningSentAt.HasValue
                && currentTime >= warningAt
                && currentTime < dueAt)
            {
                report.SLAWarningSentAt =
                    currentTime;

                report.UpdatedAt =
                    currentTime;

                warningReportCount++;

                var warningRecipientIds =
                    new HashSet<Guid>(adminIds);

                if (report.AssignedStaffId.HasValue)
                {
                    warningRecipientIds.Add(
                        report.AssignedStaffId.Value);
                }

                _notificationService.AddMany(
                    userIds: warningRecipientIds,
                    reportId: report.Id,
                    type: NotificationType.SLAWarning,
                    title: "Báo cáo sắp hết hạn SLA",
                    message:
                        $"Báo cáo {report.Id} "
                        + $"sắp hết hạn SLA lúc "
                        + $"{dueAt:dd/MM/yyyy HH:mm} UTC.",
                    createdAt: currentTime);

                createdNotificationCount +=
                    warningRecipientIds.Count;

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
                        RecipientCount =
                            warningRecipientIds.Count
                    });
            }

            /*
             * Quá hạn SLA.
             *
             * SLABreachedNotifiedAt bảo đảm mỗi chu kỳ SLA
             * chỉ xử lý breach một lần.
             */
            if (!report.SLABreachedNotifiedAt.HasValue
                && currentTime >= dueAt)
            {
                report.SLABreachedNotifiedAt =
                    currentTime;

                report.IsEscalated =
                    true;

                report.EscalatedAt =
                    currentTime;

                report.UpdatedAt =
                    currentTime;

                breachedReportCount++;
                escalatedReportCount++;

                /*
                 * Thông báo cho Staff phụ trách.
                 */
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

                /*
                 * Thông báo cho Citizen.
                 */
                _notificationService.Add(
                    userId:
                        report.CitizenId,

                    reportId:
                        report.Id,

                    type:
                        NotificationType.SLABreached,

                    title:
                        "Báo cáo đang xử lý chậm",

                    message:
                        "Báo cáo của bạn đã quá thời hạn "
                        + "xử lý dự kiến và đang được hệ thống "
                        + "chuyển cấp theo dõi.",

                    createdAt:
                        currentTime);

                createdNotificationCount++;

                /*
                 * Thông báo Escalated cho tất cả Admin
                 * đang hoạt động.
                 */
                _notificationService.AddMany(
                    userIds:
                        adminIds,

                    reportId:
                        report.Id,

                    type:
                        NotificationType.Escalated,

                    title:
                        "Báo cáo SLA bị Escalated",

                    message:
                        $"Báo cáo {report.Id} "
                        + "đã quá hạn SLA và cần Admin theo dõi.",

                    createdAt:
                        currentTime);

                createdNotificationCount +=
                    adminIds.Count;

                /*
                 * SLA breach không thay đổi ReportStatus,
                 * vì vậy OldStatus và NewStatus giống nhau.
                 */
                _dbContext.StatusUpdates.Add(
                    new StatusUpdate
                    {
                        ReportId =
                            report.Id,

                        UpdatedByUserId =
                            null,

                        OldStatus =
                            report.Status,

                        NewStatus =
                            report.Status,

                        Note =
                            "Hệ thống phát hiện báo cáo "
                            + "đã quá hạn SLA và chuyển cấp "
                            + "theo dõi.",

                        CreatedAt =
                            currentTime
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
                        report.IsEscalated,
                        report.EscalatedAt,
                        AssignedStaffNotified =
                            report.AssignedStaffId.HasValue,
                        CitizenNotified = true,
                        AdminRecipientCount =
                            adminIds.Count
                    });
            }
        }

        if (warningReportCount > 0
            || breachedReportCount > 0)
        {
            /*
             * Report, Notification, StatusUpdate và AuditLog
             * được lưu chung trong một lần SaveChangesAsync.
             */
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
