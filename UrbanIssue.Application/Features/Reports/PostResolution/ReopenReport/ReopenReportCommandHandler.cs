using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.PostResolution.ReopenReport;

public sealed class ReopenReportCommandHandler
    : IRequestHandler<
        ReopenReportCommand,
        PostResolutionActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public ReopenReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
    }

    public async Task<PostResolutionActionResult> Handle(
        ReopenReportCommand request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item => item.Id == request.ReportId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status != ReportStatus.Resolved)
        {
            throw new ConflictException(
                "Chỉ có thể mở lại báo cáo đang ở trạng thái Resolved.");
        }

        if (!report.ComplaintSubmittedAt.HasValue
            || string.IsNullOrWhiteSpace(report.ComplaintReason))
        {
            throw new ConflictException(
                "Báo cáo không có khiếu nại đang chờ xử lý.");
        }

        if (!report.AssignedStaffId.HasValue)
        {
            throw new ConflictException(
                "Báo cáo chưa có Staff phụ trách để tiếp tục xử lý.");
        }

        if (!report.AppliedSLAHours.HasValue
            || report.AppliedSLAHours.Value <= 0)
        {
            throw new ConflictException(
                "Báo cáo chưa có SLA hợp lệ để khởi động lại.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        /*
         * Lưu dữ liệu cũ trước khi reset để ghi timeline
         * và Audit Log.
         */
        var oldResolvedAt = report.ResolvedAt;
        var oldDueAt = report.DueAt;
        var complaintSubmittedAt = report.ComplaintSubmittedAt;
        var complaintReason = report.ComplaintReason.Trim();
        var adminReason = request.Reason.Trim();
        var assignedStaffId = report.AssignedStaffId.Value;

        report.Status = ReportStatus.InProgress;

        report.ReopenedAt = currentTime;
        report.ReopenedByUserId = adminId;
        report.ReopenReason = adminReason;

        /*
         * Khi Staff giải quyết lại, ResolvedAt
         * sẽ được ghi bằng thời điểm mới.
         */
        report.ResolvedAt = null;
        report.ClosedAt = null;

        /*
         * Khởi động lại SLA theo snapshot đã áp dụng
         * trước đó.
         */
        report.SLAStartedAt = currentTime;
        report.DueAt = currentTime.AddHours(
            report.AppliedSLAHours.Value);

        report.SLAWarningSentAt = null;
        report.SLABreachedNotifiedAt = null;
        report.IsEscalated = false;
        report.EscalatedAt = null;

        /*
         * Xóa khiếu nại đang chờ khỏi Report.
         * Nội dung lịch sử vẫn tồn tại trong
         * StatusUpdate và AuditLog.
         */
        report.ComplaintSubmittedAt = null;
        report.ComplaintReason = null;

        report.UpdatedAt = currentTime;

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.InProgress,

                Note =
                    $"Admin mở lại báo cáo. "
                    + $"Khiếu nại của Citizen: {complaintReason}. "
                    + $"Lý do mở lại: {adminReason}",

                CreatedAt = currentTime
            });

        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.ReportReopened,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                OldStatus = oldStatus,
                NewStatus = report.Status,

                ComplaintSubmittedAt =
                    complaintSubmittedAt,

                ComplaintReason =
                    complaintReason,

                AdminReason =
                    adminReason,

                report.AssignedStaffId,
                report.AppliedSLAHours,

                OldResolvedAt =
                    oldResolvedAt,

                OldDueAt =
                    oldDueAt,

                report.ReopenedAt,
                report.SLAStartedAt,

                NewDueAt =
                    report.DueAt
            });

        /*
         * Citizen và Staff nhận nội dung riêng,
         * không dùng AddMany để tránh gửi trùng.
         */
        _notificationService.Add(
            userId: report.CitizenId,
            reportId: report.Id,
            type: NotificationType.ReportReopened,
            title: "Khiếu nại đã được chấp nhận",
            message:
                "Báo cáo của bạn đã được mở lại "
                + "và tiếp tục xử lý.",
            createdAt: currentTime);

        _notificationService.Add(
            userId: assignedStaffId,
            reportId: report.Id,
            type: NotificationType.ReportReopened,
            title: "Báo cáo cần xử lý lại",
            message:
                "Báo cáo đã được Admin mở lại "
                + "sau khi xem xét khiếu nại.",
            createdAt: currentTime);

        /*
         * Report, StatusUpdate, AuditLog và Notification
         * được lưu chung trong một lần SaveChangesAsync.
         */
        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new PostResolutionActionResult(
            ReportId: report.Id,
            Status: report.Status,
            ComplaintSubmittedAt:
                report.ComplaintSubmittedAt,
            ComplaintDeadline: null,
            ClosedAt: report.ClosedAt,
            ReopenedAt: report.ReopenedAt,
            DueAt: report.DueAt);
    }
}
