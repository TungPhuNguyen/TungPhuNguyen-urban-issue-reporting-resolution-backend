using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public ProcessSlaMonitoringCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProcessSlaMonitoringResult> Handle(
        ProcessSlaMonitoringCommand request,
        CancellationToken cancellationToken)
    {
        var currentTime = request.CurrentTime;

        /*
         * Chỉ giám sát các Report:
         *
         * - Accepted hoặc InProgress
         * - Đã có SLA snapshot
         * - Có DueAt
         */
        var reports = await _dbContext.Reports
            .Where(report =>
                (
                    report.Status == ReportStatus.Accepted
                    || report.Status == ReportStatus.InProgress
                )
                && report.SLAStartedAt.HasValue
                && report.AppliedSLAHours.HasValue
                && report.DueAt.HasValue)
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
                user.Role.Name == "Admin")
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
             * - Đã qua 80% thời gian SLA
             * - Chưa quá hạn
             * - Chưa gửi cảnh báo trước đó
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
                    new HashSet<Guid>(
                        adminIds);

                if (report.AssignedStaffId.HasValue)
                {
                    warningRecipientIds.Add(
                        report.AssignedStaffId.Value);
                }

                foreach (var userId in warningRecipientIds)
                {
                    _dbContext.Notifications.Add(
                        CreateNotification(
                            userId: userId,
                            reportId: report.Id,
                            type: NotificationType.SLAWarning,
                            title: "Báo cáo sắp hết hạn SLA",
                            message:
                                $"Báo cáo {report.Id} "
                                + $"sắp hết hạn SLA lúc {dueAt:dd/MM/yyyy HH:mm} UTC.",
                            createdAt: currentTime));

                    createdNotificationCount++;
                }
            }

            /*
             * Xử lý khi quá hạn.
             *
             * SLABreachedNotifiedAt ngăn job gửi
             * thông báo trùng trong lần chạy sau.
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
                    _dbContext.Notifications.Add(
                        CreateNotification(
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
                                currentTime));

                    createdNotificationCount++;
                }

                /*
                 * Thông báo Citizen về việc xử lý bị trễ.
                 */
                _dbContext.Notifications.Add(
                    CreateNotification(
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
                            currentTime));

                createdNotificationCount++;

                /*
                 * Escalation notification cho tất cả Admin.
                 */
                foreach (var adminId in adminIds)
                {
                    _dbContext.Notifications.Add(
                        CreateNotification(
                            userId:
                                adminId,

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
                                currentTime));

                    createdNotificationCount++;
                }

                /*
                 * Ghi vào timeline.
                 *
                 * OldStatus và NewStatus giống nhau vì SLA breach
                 * không trực tiếp thay đổi trạng thái xử lý.
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
                            "Hệ thống phát hiện báo cáo đã quá hạn SLA "
                            + "và chuyển cấp theo dõi.",

                        CreatedAt =
                            currentTime
                    });
            }
        }

        if (warningReportCount > 0
            || breachedReportCount > 0)
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

    private static Notification CreateNotification(
        Guid userId,
        Guid reportId,
        NotificationType type,
        string title,
        string message,
        DateTime createdAt)
    {
        return new Notification
        {
            UserId =
                userId,

            ReportId =
                reportId,

            Type =
                type,

            Title =
                title,

            Message =
                message,

            IsRead =
                false,

            CreatedAt =
                createdAt,

            ReadAt =
                null
        };
    }
}
