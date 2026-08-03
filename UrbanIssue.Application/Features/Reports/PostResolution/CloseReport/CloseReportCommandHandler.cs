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
using UrbanIssue.Domain.Constants;

namespace UrbanIssue.Application.Features.Reports.PostResolution.CloseReport;

public sealed class CloseReportCommandHandler
    : IRequestHandler<
        CloseReportCommand,
        PostResolutionActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public CloseReportCommandHandler(
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
     CloseReportCommand request,
     CancellationToken cancellationToken)
    {
        var currentUserId =
            _currentUserService.UserId;

        var currentRole =
            _currentUserService.Role;

        var isAdmin = string.Equals(
            currentRole,
            RoleNames.Admin,
            StringComparison.OrdinalIgnoreCase);

        var isCitizen = string.Equals(
            currentRole,
            RoleNames.Citizen,
            StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isCitizen)
        {
            throw new UnauthorizedAccessException(
                "Bạn không có quyền đóng báo cáo.");
        }

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item => item.Id == request.ReportId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        /*
         * Citizen chỉ được đóng báo cáo do chính mình tạo.
         * Admin được đóng mọi báo cáo hợp lệ.
         */
        if (
            isCitizen &&
            report.CitizenId != currentUserId)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status != ReportStatus.Resolved)
        {
            throw new ConflictException(
                "Chỉ có thể đóng báo cáo đang ở trạng thái Resolved.");
        }

        var hasPendingComplaint = await _dbContext.Complaints
            .AnyAsync(
                complaint => complaint.ReportId == report.Id
                    && complaint.Status == ComplaintStatus.Pending,
                cancellationToken);

        if (hasPendingComplaint)
        {
            throw new ConflictException(
                "Không thể đóng báo cáo đang có khiếu nại "
                + "chờ Admin xem xét. Admin phải chấp nhận mở lại "
                + "hoặc từ chối khiếu nại.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        var actorLabel = isAdmin
            ? "Admin"
            : "Citizen";

        var note = string.IsNullOrWhiteSpace(
            request.Note)
            ? $"{actorLabel} đã xác nhận kết quả xử lý "
              + "và đóng báo cáo."
            : request.Note.Trim();

        report.Status = ReportStatus.Closed;
        report.ClosedAt = currentTime;
        report.UpdatedAt = currentTime;

        /*
         * Không thay đổi ResolutionNote hoặc proof images.
         * Chỉ cập nhật trạng thái và thời gian đóng.
         */
        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = currentUserId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Closed,
                Note = note,
                CreatedAt = currentTime
            });

        _auditLogService.Add(
            userId: currentUserId,
            action: AuditActions.ReportClosed,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                ActorRole = currentRole,
                OldStatus = oldStatus,
                NewStatus = report.Status,
                report.ResolvedAt,
                report.ClosedAt,
                Note = note
            });

        _notificationService.Add(
            userId: report.CitizenId,
            reportId: report.Id,
            type: NotificationType.ReportClosed,
            title: "Báo cáo đã được đóng",
            message: isAdmin
                ? "Admin đã xác nhận và đóng báo cáo."
                : "Bạn đã xác nhận kết quả xử lý "
                  + "và đóng báo cáo thành công.",
            createdAt: currentTime);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new PostResolutionActionResult(
            ReportId: report.Id,
            Status: report.Status,

            ComplaintSubmittedAt:
                report.ComplaintSubmittedAt,

            ComplaintDeadline:
                report.ResolvedAt?.AddDays(7),

            ClosedAt:
                report.ClosedAt,

            ReopenedAt:
                report.ReopenedAt,

            DueAt:
                report.DueAt,

            ReportCode:
                report.ReportCode);
    }
}
