using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.AddProgressNote;

public sealed class AddProgressNoteCommandHandler
    : IRequestHandler<
        AddProgressNoteCommand,
        StaffProgressUpdateResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public AddProgressNoteCommandHandler(
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

    public async Task<StaffProgressUpdateResult> Handle(
        AddProgressNoteCommand request,
        CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId;

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item =>
                    item.Id == request.ReportId
                    && item.AssignedStaffId == staffId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy báo cáo hoặc báo cáo "
                + "không thuộc Staff hiện tại.");
        }

        if (report.Status != ReportStatus.InProgress)
        {
            throw new ConflictException(
                "Chỉ có thể thêm ghi chú tiến độ cho báo cáo "
                + "đang ở trạng thái InProgress.");
        }

        var currentTime = DateTime.UtcNow;
        var note = request.Note.Trim();

        report.UpdatedAt = currentTime;

        var statusUpdate = new StatusUpdate
        {
            ReportId = report.Id,
            UpdatedByUserId = staffId,
            OldStatus = report.Status,
            NewStatus = report.Status,
            Note = note,
            CreatedAt = currentTime
        };

        _dbContext.StatusUpdates.Add(statusUpdate);

        _auditLogService.Add(
            userId: staffId,
            action: AuditActions.ReportProgressNoteAdded,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                report.Status,
                report.DepartmentId,
                report.AssignedStaffId,
                Note = note,
                CreatedAt = currentTime
            });

        _notificationService.Add(
            userId: report.CitizenId,
            reportId: report.Id,
            type: NotificationType.ReportStatusChanged,
            title: "Có cập nhật tiến độ mới",
            message:
                "Nhân viên phụ trách đã thêm ghi chú "
                + "tiến độ cho báo cáo của bạn.",
            createdAt: currentTime);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new StaffProgressUpdateResult(
            ReportId: report.Id,
            StatusUpdateId: statusUpdate.Id,
            Status: report.Status,
            Note: statusUpdate.Note,
            ImageUrls: [],
            CreatedAt: statusUpdate.CreatedAt,
            UpdatedAt: report.UpdatedAt);
    }
}
