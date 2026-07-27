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
        var citizenId = _currentUserService.UserId;

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item =>
                    item.Id == request.ReportId
                    && item.CitizenId == citizenId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status != ReportStatus.Resolved)
        {
            throw new ConflictException(
                "Chỉ có thể đóng báo cáo đang ở trạng thái Resolved.");
        }

        if (report.ComplaintSubmittedAt.HasValue)
        {
            throw new ConflictException(
                "Không thể đóng báo cáo đang có khiếu nại "
                + "chờ Admin xem xét.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        var note = string.IsNullOrWhiteSpace(request.Note)
            ? "Citizen đã xác nhận kết quả xử lý và đóng báo cáo."
            : request.Note.Trim();

        report.Status = ReportStatus.Closed;
        report.ClosedAt = currentTime;
        report.UpdatedAt = currentTime;

        /*
         * Ghi thay đổi trạng thái vào timeline.
         */
        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = citizenId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Closed,
                Note = note,
                CreatedAt = currentTime
            });

        /*
         * Ghi Audit Log cho thao tác đóng Report.
         */
        _auditLogService.Add(
            userId: citizenId,
            action: AuditActions.ReportClosed,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                OldStatus = oldStatus,
                NewStatus = report.Status,
                report.ResolvedAt,
                report.ClosedAt,
                Note = note
            });

        /*
         * Gửi thông báo xác nhận cho Citizen.
         */
        _notificationService.Add(
            userId: report.CitizenId,
            reportId: report.Id,
            type: NotificationType.ReportClosed,
            title: "Báo cáo đã được đóng",
            message:
                "Bạn đã xác nhận kết quả xử lý "
                + "và đóng báo cáo thành công.",
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

            ComplaintDeadline:
                report.ResolvedAt?.AddDays(7),

            ClosedAt:
                report.ClosedAt,

            ReopenedAt:
                report.ReopenedAt,

            DueAt:
                report.DueAt);
    }
}
