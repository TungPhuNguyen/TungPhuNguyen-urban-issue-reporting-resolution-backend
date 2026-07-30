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
                "Chỉ có thể mở lại báo cáo "
                + "đang ở trạng thái Resolved.");
        }

        if (!report.HasSubmittedComplaint
            || !report.ComplaintSubmittedAt.HasValue
            || string.IsNullOrWhiteSpace(
                report.ComplaintReason))
        {
            throw new ConflictException(
                "Báo cáo không có khiếu nại "
                + "đang chờ xử lý.");
        }

        if (!report.AssignedStaffId.HasValue)
        {
            throw new ConflictException(
                "Báo cáo chưa có Staff phụ trách "
                + "để tiếp tục xử lý.");
        }

        if (!report.AppliedSLAHours.HasValue
            || report.AppliedSLAHours.Value <= 0)
        {
            throw new ConflictException(
                "Báo cáo chưa có SLA hợp lệ "
                + "để khởi động lại.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        var oldResolvedAt = report.ResolvedAt;
        var oldDueAt = report.DueAt;
        var complaintSubmittedAt =
            report.ComplaintSubmittedAt;
        var complaintReason =
            report.ComplaintReason.Trim();
        var adminReason =
            request.Reason.Trim();
        var assignedStaffId =
            report.AssignedStaffId.Value;

        report.Status = ReportStatus.InProgress;
        report.ReopenedAt = currentTime;
        report.ReopenedByUserId = adminId;
        report.ReopenReason = adminReason;
        report.ResolvedAt = null;
        report.ClosedAt = null;

        /*
         * Bắt đầu chu kỳ SLA mới bằng snapshot
         * AppliedSLAHours đã áp dụng trước đó.
         */
        report.SLAStartedAt = currentTime;
        report.DueAt = currentTime.AddHours(
            report.AppliedSLAHours.Value);

        report.SLAWarningSentAt = null;
        report.SLABreachedNotifiedAt = null;
        report.IsEscalated = false;
        report.EscalatedAt = null;

        /*
         * Xóa trạng thái khiếu nại đang chờ để Report
         * tiếp tục xử lý. HasSubmittedComplaint vẫn giữ
         * true nhằm chặn Citizen khiếu nại lần hai.
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
                    "Admin chấp nhận khiếu nại và mở lại "
                    + $"báo cáo. Khiếu nại: {complaintReason}. "
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
                HasSubmittedComplaint =
                    report.HasSubmittedComplaint,
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
