using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Notifications;
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
    private readonly INotificationService _notificationService;

    public AutoCloseResolvedReportsCommandHandler(
        IApplicationDbContext dbContext,
        IAuditLogService auditLogService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
    }

    public async Task<int> Handle(
        AutoCloseResolvedReportsCommand request,
        CancellationToken cancellationToken)
    {
        var resolvedBefore = request.CurrentTime.AddDays(
            -AutoClosePeriodInDays);

        /*
         * Chỉ tự động đóng các Report:
         *
         * - Đang ở trạng thái Resolved
         * - Đã giải quyết ít nhất 7 ngày
         * - Không có khiếu nại đang chờ xử lý
         */
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

            /*
             * Ghi sự kiện tự động đóng vào timeline.
             * UpdatedByUserId = null biểu thị thao tác hệ thống.
             */
            _dbContext.StatusUpdates.Add(
                new StatusUpdate
                {
                    ReportId = report.Id,
                    UpdatedByUserId = null,
                    OldStatus = oldStatus,
                    NewStatus = ReportStatus.Closed,

                    Note =
                        "Hệ thống tự động đóng báo cáo "
                        + "sau 7 ngày không có khiếu nại.",

                    CreatedAt = request.CurrentTime
                });

            /*
             * Ghi Audit Log với UserId = null
             * vì đây là tác vụ do hệ thống thực hiện.
             */
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

            /*
             * Thông báo cho Citizen rằng Report
             * đã được hệ thống tự động đóng.
             */
            _notificationService.Add(
                userId: report.CitizenId,
                reportId: report.Id,
                type: NotificationType.ReportClosed,
                title: "Báo cáo đã tự động đóng",
                message:
                    "Báo cáo đã được hệ thống tự động đóng "
                    + "sau 7 ngày không có khiếu nại.",
                createdAt: request.CurrentTime);
        }

        if (reports.Count > 0)
        {
            /*
             * Report, StatusUpdate, AuditLog và Notification
             * được lưu chung trong một lần SaveChangesAsync.
             */
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return reports.Count;
    }
}
