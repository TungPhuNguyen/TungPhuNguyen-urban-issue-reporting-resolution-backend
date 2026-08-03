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

namespace UrbanIssue.Application.Features.Reports.CancelReport;

public sealed class CancelReportCommandHandler
    : IRequestHandler<CancelReportCommand, PostResolutionActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public CancelReportCommandHandler(
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
        CancelReportCommand request,
        CancellationToken cancellationToken)
    {
        var citizenId = _currentUserService.UserId;
        var report = await _dbContext.Reports.SingleOrDefaultAsync(
            item => item.Id == request.ReportId && item.CitizenId == citizenId,
            cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status is not (ReportStatus.New or ReportStatus.Assigned))
        {
            throw new ConflictException(
                "Chỉ được hủy phản ánh trước khi Staff tiếp nhận.");
        }

        if (!report.RowVersion.SequenceEqual(request.RowVersion))
        {
            throw new ConflictException(
                "Phản ánh đã được cập nhật. Hãy tải lại dữ liệu trước khi hủy.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;
        var reason = request.Reason.Trim();
        var assignedStaffId = report.AssignedStaffId;

        report.Status = ReportStatus.Cancelled;
        report.AssignedStaffId = null;
        report.CancelledAt = currentTime;
        report.UpdatedAt = currentTime;

        _dbContext.StatusUpdates.Add(new StatusUpdate
        {
            ReportId = report.Id,
            UpdatedByUserId = citizenId,
            OldStatus = oldStatus,
            NewStatus = ReportStatus.Cancelled,
            EventType = TimelineEventType.ReportCancelled,
            Note = $"Citizen đã hủy phản ánh. Lý do: {reason}",
            CreatedAt = currentTime
        });

        _auditLogService.Add(
            citizenId,
            AuditActions.ReportCancelled,
            AuditEntityTypes.Report,
            report.Id.ToString(),
            new { report.ReportCode, OldStatus = oldStatus, Reason = reason });

        if (assignedStaffId.HasValue)
        {
            _notificationService.Add(
                assignedStaffId.Value,
                report.Id,
                NotificationType.ReportCancelled,
                "Phản ánh đã được hủy",
                $"Citizen đã hủy phản ánh {report.ReportCode} trước khi tiếp nhận.",
                currentTime);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PostResolutionActionResult(
            report.Id,
            report.Status,
            report.ComplaintSubmittedAt,
            null,
            report.ClosedAt,
            report.ReopenedAt,
            report.DueAt,
            report.ReportCode);
    }
}
