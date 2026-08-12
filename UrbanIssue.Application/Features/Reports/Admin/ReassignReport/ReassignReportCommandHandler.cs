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

namespace UrbanIssue.Application.Features.Reports.Admin.ReassignReport;

public sealed class ReassignReportCommandHandler
    : IRequestHandler<
        ReassignReportCommand,
        AdminReportActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public ReassignReportCommandHandler(
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
        ReassignReportCommand request,
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

        if (report.Status is not (
            ReportStatus.Assigned
            or ReportStatus.Accepted
            or ReportStatus.InProgress))
        {
            throw new ConflictException(
                "Chỉ có thể phân công lại báo cáo đang ở trạng thái "
                + "Assigned, Accepted hoặc InProgress.");
        }

        if (report.DepartmentId == request.DepartmentId
            && report.AssignedStaffId == request.StaffId)
        {
            throw new ConflictException(
                "Báo cáo đã được phân công đúng phòng ban và Staff này.");
        }

        var department = await _dbContext.Departments
            .AsNoTracking()
            .Where(item => item.Id == request.DepartmentId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy phòng ban có ID {request.DepartmentId}.");
        }

        if (!department.IsActive)
        {
            throw new ConflictException(
                "Không thể phân công báo cáo cho phòng ban "
                + "đã ngừng hoạt động.");
        }

        string? staffName = null;

        if (request.StaffId.HasValue)
        {
            var staff = await _dbContext.Users
                .AsNoTracking()
                .Where(user =>
                    user.Id == request.StaffId.Value
                    && user.IsActive
                    && user.DepartmentId == request.DepartmentId
                    && user.Role.Name == "Staff")
                .Select(user => new
                {
                    user.FullName
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (staff is null)
            {
                throw new ConflictException(
                    "Staff không tồn tại, đã bị khóa, không thuộc "
                    + "phòng ban đã chọn hoặc không có vai trò Staff.");
            }

            staffName = staff.FullName;
        }

        var currentTime = DateTime.UtcNow;
        var reason = request.Reason.Trim();

        /*
         * Lưu toàn bộ dữ liệu cũ trước khi cập nhật
         * để ghi timeline và Audit Log.
         */
        var oldStatus = report.Status;
        var oldDepartmentId = report.DepartmentId;
        var oldAssignedStaffId = report.AssignedStaffId;
        var oldPriority = report.Priority;
        var oldSlaConfigId = report.SLAConfigId;
        var oldSlaStartedAt = report.SLAStartedAt;
        var oldAppliedSlaHours = report.AppliedSLAHours;
        var oldDueAt = report.DueAt;
        var oldAcceptedAt = report.AcceptedAt;
        var oldIsEscalated = report.IsEscalated;

        report.DepartmentId = request.DepartmentId;
        report.AssignedStaffId = request.StaffId;
        report.Status = ReportStatus.Assigned;
        report.RequiresManualAssignment = false;

        /*
         * Staff mới phải Accept lại để xác định Priority
         * và bắt đầu một chu kỳ SLA mới.
         */
        report.Priority = null;
        report.SLAConfigId = null;
        report.SLAStartedAt = null;
        report.AppliedSLAHours = null;
        report.DueAt = null;

        report.SLAWarningSentAt = null;
        report.SLABreachedNotifiedAt = null;
        report.IsEscalated = false;
        report.EscalatedAt = null;

        report.AcceptedAt = null;
        report.UpdatedAt = currentTime;

        var targetName = staffName is null
            ? department.Name
            : $"{staffName} - {department.Name}";

        /*
         * Ghi lịch sử thay đổi trạng thái.
         */
        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Assigned,

                Note =
                    $"Admin phân công lại báo cáo cho {targetName}. "
                    + $"Lý do: {reason}",

                CreatedAt = currentTime
            });

        /*
         * Ghi Audit Log trước và sau khi Reassign.
         */
        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.ReportReassigned,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                OldStatus = oldStatus,
                NewStatus = report.Status,

                OldDepartmentId = oldDepartmentId,
                NewDepartmentId = report.DepartmentId,
                NewDepartmentName = department.Name,

                OldAssignedStaffId = oldAssignedStaffId,
                NewAssignedStaffId = report.AssignedStaffId,
                NewAssignedStaffName = staffName,

                OldPriority = oldPriority,
                NewPriority = report.Priority,

                OldSLAConfigId = oldSlaConfigId,
                NewSLAConfigId = report.SLAConfigId,

                OldSLAStartedAt = oldSlaStartedAt,
                NewSLAStartedAt = report.SLAStartedAt,

                OldAppliedSLAHours = oldAppliedSlaHours,
                NewAppliedSLAHours = report.AppliedSLAHours,

                OldDueAt = oldDueAt,
                NewDueAt = report.DueAt,

                OldAcceptedAt = oldAcceptedAt,
                NewAcceptedAt = report.AcceptedAt,

                OldIsEscalated = oldIsEscalated,
                NewIsEscalated = report.IsEscalated,

                Reason = reason
            });

        /*
         * Thông báo cho Staff cũ khi Report
         * không còn thuộc Staff đó.
         */
        if (oldAssignedStaffId.HasValue
            && oldAssignedStaffId != report.AssignedStaffId)
        {
            _notificationService.Add(
                userId: oldAssignedStaffId.Value,
                reportId: report.Id,
                type: NotificationType.ReportReassigned,
                title: "Báo cáo đã được điều chuyển",
                message:
                    "Báo cáo trước đây do bạn phụ trách "
                    + "đã được Admin phân công lại.",
                createdAt: currentTime);
        }

        /*
         * Nếu phân công trực tiếp, chỉ thông báo Staff mới.
         */
        if (report.AssignedStaffId.HasValue)
        {
            _notificationService.Add(
                userId: report.AssignedStaffId.Value,
                reportId: report.Id,
                type: NotificationType.ReportReassigned,
                title: "Bạn được phân công lại báo cáo",
                message:
                    "Bạn được phân công xử lý báo cáo tại "
                    + $"{report.AddressText ?? "địa điểm chưa xác định"}.",
                createdAt: currentTime);
        }
        else
        {
            /*
             * Nếu chỉ phân công Department, thông báo cho
             * toàn bộ Staff đang hoạt động trong Department mới.
             *
             * Staff cũ bị loại khỏi danh sách vì đã nhận
             * thông báo điều chuyển riêng ở trên.
             */
            var departmentStaffIds = await _dbContext.Users
                .AsNoTracking()
                .Where(user =>
                    user.IsActive
                    && user.Role.Name == "Staff"
                    && user.DepartmentId == report.DepartmentId
                    && (
                        !oldAssignedStaffId.HasValue
                        || user.Id != oldAssignedStaffId.Value
                    ))
                .Select(user => user.Id)
                .ToListAsync(cancellationToken);

            _notificationService.AddMany(
                userIds: departmentStaffIds,
                reportId: report.Id,
                type: NotificationType.ReportReassigned,
                title: "Phòng ban nhận báo cáo điều chuyển",
                message:
                    $"Báo cáo đã được điều chuyển đến {department.Name}.",
                createdAt: currentTime);
        }

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
            DepartmentId: report.DepartmentId,
            DepartmentName: department.Name,
            AssignedStaffId: report.AssignedStaffId,
            AssignedStaffName: staffName,
            Priority: report.Priority,
            RequiresManualAssignment:
                report.RequiresManualAssignment,
            UpdatedAt: report.UpdatedAt);
    }
}
