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

namespace UrbanIssue.Application.Features.Reports.PostResolution.DismissComplaint;

public sealed class DismissComplaintCommandHandler
    : IRequestHandler<
        DismissComplaintCommand,
        PostResolutionActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public DismissComplaintCommandHandler(
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
        DismissComplaintCommand request,
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
                "Chỉ có thể xử lý khiếu nại của báo cáo "
                + "đang ở trạng thái Resolved.");
        }

        var complaint = await _dbContext.Complaints
            .SingleOrDefaultAsync(
                item => item.ReportId == report.Id
                    && item.Status == ComplaintStatus.Pending,
                cancellationToken);

        if (complaint is null)
        {
            throw new ConflictException(
                "Báo cáo không có khiếu nại "
                + "đang chờ Admin xem xét.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;
        var adminReason = request.Reason.Trim();
        var complaintSubmittedAt = complaint.CreatedAt;
        var complaintReason = complaint.Reason;

        report.Status = ReportStatus.Closed;
        report.ClosedAt = currentTime;
        report.UpdatedAt = currentTime;

        complaint.Status = ComplaintStatus.Rejected;
        complaint.AdminDecisionReason = adminReason;
        complaint.ResolvedByAdminId = adminId;
        complaint.ResolvedAt = currentTime;

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Closed,
                EventType = TimelineEventType.ComplaintRejected,
                Note =
                    "Admin không chấp nhận khiếu nại và "
                    + $"đóng báo cáo. Khiếu nại: "
                    + $"{complaintReason}. Lý do: "
                    + $"{adminReason}",
                CreatedAt = currentTime
            });

        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.ComplaintDismissed,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                OldStatus = oldStatus,
                NewStatus = report.Status,
                report.HasSubmittedComplaint,
                ComplaintSubmittedAt =
                    complaintSubmittedAt,
                ComplaintReason =
                    complaintReason,
                AdminReason =
                    adminReason,
                report.ClosedAt
            });

        _notificationService.Add(
            userId: report.CitizenId,
            reportId: report.Id,
            type: NotificationType.ReportClosed,
            title: "Khiếu nại không được chấp nhận",
            message:
                "Admin đã xem xét khiếu nại và đóng "
                + $"báo cáo. Lý do: {adminReason}",
            createdAt: currentTime);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new PostResolutionActionResult(
            ReportId: report.Id,
            Status: report.Status,
            ComplaintSubmittedAt: null,
            ComplaintDeadline: null,
            ClosedAt: report.ClosedAt,
            ReopenedAt: report.ReopenedAt,
            DueAt: report.DueAt,
            ReportCode: report.ReportCode,
            ComplaintId: complaint.Id,
            ComplaintStatus: complaint.Status);
    }
}
