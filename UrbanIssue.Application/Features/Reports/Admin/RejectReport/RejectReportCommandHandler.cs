using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Admin.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Admin.RejectReport;

public sealed class RejectReportCommandHandler
    : IRequestHandler<
        RejectReportCommand,
        AdminReportActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public RejectReportCommandHandler(
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

    public async Task<AdminReportActionResult> Handle(
        RejectReportCommand request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        /*
         * Tải kèm Department và AssignedStaff để dùng
         * cho kết quả trả về mà không cần truy vấn bổ sung.
         */
        var report = await _dbContext.Reports
            .Include(item => item.Department)
            .Include(item => item.AssignedStaff)
            .SingleOrDefaultAsync(
                item => item.Id == request.ReportId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        /*
         * Chỉ cho Reject các trạng thái:
         *
         * New
         * Assigned
         * Accepted
         * InProgress
         */
        if (report.Status is
            ReportStatus.Resolved
            or ReportStatus.Closed
            or ReportStatus.Rejected
            or ReportStatus.Cancelled)
        {
            throw new ConflictException(
                "Không thể từ chối báo cáo đã Resolved, "
                + "Closed, Rejected hoặc Cancelled.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;
        var reason = request.Reason.Trim();

        /*
         * Giữ lại DepartmentId, AssignedStaffId và thông tin SLA
         * để phục vụ lịch sử và Audit Log.
         *
         * Background SLA không tiếp tục xử lý vì trạng thái
         * đã chuyển sang Rejected.
         */
        report.Status = ReportStatus.Rejected;
        report.RejectedAt = currentTime;
        report.RejectedByUserId = adminId;
        report.RejectedReason = reason;
        report.RequiresManualAssignment = false;
        report.UpdatedAt = currentTime;

        /*
         * Ghi lịch sử thay đổi trạng thái.
         */
        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Rejected,
                Note =
                    $"Admin từ chối báo cáo. Lý do: {reason}",
                CreatedAt = currentTime
            });

        /*
         * Ghi Audit Log cho thao tác Reject.
         */
        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.ReportRejected,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                OldStatus = oldStatus,
                NewStatus = report.Status,

                report.CategoryId,
                report.AreaId,

                report.DepartmentId,
                DepartmentName =
                    report.Department?.Name,

                report.AssignedStaffId,
                AssignedStaffName =
                    report.AssignedStaff?.FullName,

                report.Priority,
                report.DueAt,

                report.RejectedAt,
                RejectedReason = reason
            });

        /*
         * Thông báo kết quả cho Citizen.
         */
        _notificationService.Add(
            userId: report.CitizenId,
            reportId: report.Id,
            type: NotificationType.ReportRejected,
            title: "Báo cáo đã bị từ chối",
            message:
                "Báo cáo của bạn đã bị từ chối. "
                + $"Lý do: {reason}",
            createdAt: currentTime);

        /*
         * Report, StatusUpdate, AuditLog và Notification
         * được lưu chung trong một lần SaveChangesAsync.
         */
        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AdminReportActionResult(
            ReportId: report.Id,
            ReportCode: report.ReportCode,
            Status: report.Status,

            DepartmentId:
                report.DepartmentId,

            DepartmentName:
                report.Department?.Name,

            AssignedStaffId:
                report.AssignedStaffId,

            AssignedStaffName:
                report.AssignedStaff?.FullName,

            Priority:
                report.Priority,

            RequiresManualAssignment:
                report.RequiresManualAssignment,

            UpdatedAt:
                report.UpdatedAt);
    }
}
