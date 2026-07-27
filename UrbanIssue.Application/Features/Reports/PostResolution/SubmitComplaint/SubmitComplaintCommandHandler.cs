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

namespace UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;

public sealed class SubmitComplaintCommandHandler
    : IRequestHandler<
        SubmitComplaintCommand,
        PostResolutionActionResult>
{
    private const int ComplaintPeriodInDays = 7;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public SubmitComplaintCommandHandler(
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
        SubmitComplaintCommand request,
        CancellationToken cancellationToken)
    {
        var citizenId = _currentUserService.UserId;

        /*
         * Lọc theo cả ReportId và CitizenId để Citizen
         * không thể khiếu nại báo cáo của người khác.
         */
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
                "Chỉ có thể khiếu nại báo cáo đang ở trạng thái Resolved.");
        }

        if (!report.ResolvedAt.HasValue)
        {
            throw new ConflictException(
                "Báo cáo chưa có thời điểm hoàn tất xử lý.");
        }

        if (report.ComplaintSubmittedAt.HasValue)
        {
            throw new ConflictException(
                "Báo cáo đã có khiếu nại đang chờ Admin xem xét.");
        }

        var currentTime = DateTime.UtcNow;

        var complaintDeadline =
            report.ResolvedAt.Value.AddDays(
                ComplaintPeriodInDays);

        /*
         * Tại đúng thời điểm deadline vẫn cho phép gửi.
         * Chỉ từ chối khi đã vượt quá deadline.
         */
        if (currentTime > complaintDeadline)
        {
            throw new ConflictException(
                "Đã hết thời hạn 7 ngày để gửi khiếu nại.");
        }

        var oldStatus = report.Status;
        var reason = request.Reason.Trim();

        report.ComplaintSubmittedAt = currentTime;
        report.ComplaintReason = reason;
        report.UpdatedAt = currentTime;

        /*
         * Khiếu nại chưa làm thay đổi trạng thái.
         * Report vẫn ở trạng thái Resolved cho đến khi
         * Admin xem xét và thực hiện Reopen.
         */
        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = citizenId,
                OldStatus = oldStatus,
                NewStatus = report.Status,
                Note = $"Citizen đã gửi khiếu nại: {reason}",
                CreatedAt = currentTime
            });

        /*
         * Ghi lại thao tác gửi khiếu nại trong Audit Log.
         */
        _auditLogService.Add(
            userId: citizenId,
            action: AuditActions.ComplaintSubmitted,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                report.Status,
                report.ResolvedAt,
                report.ComplaintSubmittedAt,
                ComplaintReason = reason,
                ComplaintDeadline = complaintDeadline,
                report.AssignedStaffId
            });

        /*
         * Thông báo cho tất cả Admin đang hoạt động.
         */
        var recipientIds = await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.IsActive
                && user.Role.Name == "Admin")
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        var distinctRecipientIds =
            new HashSet<Guid>(recipientIds);

        /*
         * Thông báo thêm cho Staff đang phụ trách,
         * nhưng chỉ khi tài khoản Staff còn hoạt động.
         */
        if (report.AssignedStaffId.HasValue)
        {
            var assignedStaffIsActive =
                await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(
                        user =>
                            user.Id == report.AssignedStaffId.Value
                            && user.IsActive
                            && user.Role.Name == "Staff",
                        cancellationToken);

            if (assignedStaffIsActive)
            {
                distinctRecipientIds.Add(
                    report.AssignedStaffId.Value);
            }
        }

        _notificationService.AddMany(
            userIds: distinctRecipientIds,
            reportId: report.Id,
            type: NotificationType.ComplaintSubmitted,
            title: "Có khiếu nại mới",
            message:
                $"Citizen đã gửi khiếu nại đối với báo cáo "
                + $"{report.Id}.",
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
                complaintDeadline,

            ClosedAt:
                report.ClosedAt,

            ReopenedAt:
                report.ReopenedAt,

            DueAt:
                report.DueAt);
    }
}
