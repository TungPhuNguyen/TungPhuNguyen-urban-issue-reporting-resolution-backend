using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;
using UrbanIssue.Application.Common.Interfaces.Notifications;

namespace UrbanIssue.Application.Features.Reports.Staff.AcceptReport;

public sealed class AcceptReportCommandHandler
    : IRequestHandler<AcceptReportCommand, StaffReportActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public AcceptReportCommandHandler(
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

    public async Task<StaffReportActionResult> Handle(
        AcceptReportCommand request,
        CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId;

        var staff = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == staffId)
            .Select(user => new
            {
                user.DepartmentId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (staff is null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy tài khoản Staff.");
        }

        if (!staff.DepartmentId.HasValue)
        {
            throw new ConflictException(
                "Tài khoản Staff chưa được gán phòng ban.");
        }

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item =>
                    item.Id == request.ReportId
                    && item.DepartmentId == staff.DepartmentId.Value,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status != ReportStatus.Assigned)
        {
            throw new ConflictException(
                "Chỉ có thể tiếp nhận báo cáo đang ở trạng thái Assigned.");
        }

        if (report.AssignedStaffId.HasValue
            && report.AssignedStaffId.Value != staffId)
        {
            throw new ConflictException(
                "Báo cáo đã được phân công cho Staff khác.");
        }

        var slaConfig = await _dbContext.SLAConfigs
            .AsNoTracking()
            .Where(config =>
                config.CategoryId == report.CategoryId
                && config.Priority == request.Priority)
            .Select(config => new
            {
                config.Id,
                config.DurationHours
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (slaConfig is null)
        {
            throw new ConflictException(
                "Chưa cấu hình SLA cho loại sự cố và mức độ ưu tiên này.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        report.AssignedStaffId = staffId;
        report.Priority = request.Priority;
        report.Status = ReportStatus.Accepted;

        report.SLAConfigId = slaConfig.Id;
        report.SLAStartedAt = currentTime;
        report.AppliedSLAHours = slaConfig.DurationHours;
        report.DueAt = currentTime.AddHours(
            slaConfig.DurationHours);

        /*
         * Reset các cờ giám sát SLA để bảo đảm
         * Report bắt đầu một chu kỳ SLA mới.
         */
        report.SLAWarningSentAt = null;
        report.SLABreachedNotifiedAt = null;
        report.IsEscalated = false;
        report.EscalatedAt = null;

        report.AcceptedAt = currentTime;
        report.UpdatedAt = currentTime;

        var note = string.IsNullOrWhiteSpace(request.Note)
            ? $"Staff đã tiếp nhận báo cáo với mức ưu tiên {request.Priority}."
            : request.Note.Trim();

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = staffId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Accepted,
                Note = note,
                CreatedAt = currentTime
            });

        /*
         * Handler Accept chỉ ghi audit REPORT_ACCEPTED.
         * Audit StartProcessing và Resolve thuộc các handler riêng.
         */
        _auditLogService.Add(
            userId: staffId,
            action: AuditActions.ReportAccepted,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                OldStatus = oldStatus,
                NewStatus = report.Status,

                report.Priority,
                report.DepartmentId,
                report.AssignedStaffId,

                report.SLAConfigId,
                report.AppliedSLAHours,
                report.SLAStartedAt,
                report.DueAt,

                Note = note
            });
        _notificationService.Add(
    userId: report.CitizenId,
    reportId: report.Id,
    type: NotificationType.ReportStatusChanged,
    title: "Báo cáo đã được tiếp nhận",
    message:
        $"Báo cáo của bạn đã được tiếp nhận "
        + $"với mức ưu tiên {report.Priority}.",
    createdAt: currentTime);

        /*
         * Report, StatusUpdate và AuditLog được lưu
         * trong cùng một lần SaveChangesAsync.
         */
        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new StaffReportActionResult(
            Id: report.Id,
            Status: report.Status,
            Priority: report.Priority,
            AssignedStaffId: report.AssignedStaffId,
            SlaConfigId: report.SLAConfigId,
            AppliedSlaHours: report.AppliedSLAHours,
            SlaStartedAt: report.SLAStartedAt,
            DueAt: report.DueAt,
            UpdatedAt: report.UpdatedAt);
    }
}
