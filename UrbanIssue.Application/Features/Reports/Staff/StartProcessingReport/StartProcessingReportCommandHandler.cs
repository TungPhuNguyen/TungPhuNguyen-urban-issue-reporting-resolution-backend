using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.StartProcessingReport;

public sealed class StartProcessingReportCommandHandler
    : IRequestHandler<
        StartProcessingReportCommand,
        StaffReportActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;


    public StartProcessingReportCommandHandler(
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
        StartProcessingReportCommand request,
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
                "Không tìm thấy báo cáo hoặc báo cáo không thuộc Staff hiện tại.");
        }

        if (report.Status != ReportStatus.Accepted)
        {
            throw new ConflictException(
                "Chỉ có thể bắt đầu xử lý báo cáo đã được tiếp nhận.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        report.Status = ReportStatus.InProgress;
        report.UpdatedAt = currentTime;

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = staffId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.InProgress,
                Note = string.IsNullOrWhiteSpace(request.Note)
                    ? "Staff đã bắt đầu xử lý báo cáo."
                    : request.Note.Trim(),
                CreatedAt = currentTime
            });
        _notificationService.Add(
    userId: report.CitizenId,
    reportId: report.Id,
    type: NotificationType.ReportStatusChanged,
    title: "Báo cáo đang được xử lý",
    message:
        "Nhân viên phụ trách đã bắt đầu "
        + "xử lý báo cáo của bạn.",
    createdAt: currentTime);

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
