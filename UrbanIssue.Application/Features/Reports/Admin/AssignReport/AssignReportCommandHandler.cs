using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Admin.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Admin.AssignReport;

public sealed class AssignReportCommandHandler
    : IRequestHandler<
        AssignReportCommand,
        AdminReportActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public AssignReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<AdminReportActionResult> Handle(
        AssignReportCommand request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                report => report.Id == request.ReportId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status != ReportStatus.New)
        {
            throw new ConflictException(
                "Chỉ có thể phân công báo cáo đang ở trạng thái New.");
        }

        var department = await _dbContext.Departments
            .AsNoTracking()
            .Where(item =>
                item.Id == request.DepartmentId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.IsActive
            })
            .SingleOrDefaultAsync(
                cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy phòng ban có ID {request.DepartmentId}.");
        }

        if (!department.IsActive)
        {
            throw new ConflictException(
                "Không thể phân công báo cáo cho phòng ban đã ngừng hoạt động.");
        }

        string? staffName = null;

        if (request.StaffId.HasValue)
        {
            var staff = await _dbContext.Users
                .AsNoTracking()
                .Where(user =>
                    user.Id == request.StaffId.Value
                    && user.DepartmentId == request.DepartmentId
                    && user.Role.Name == "Staff")
                .Select(user => new
                {
                    user.FullName
                })
                .SingleOrDefaultAsync(
                    cancellationToken);

            if (staff is null)
            {
                throw new ConflictException(
                    "Staff không tồn tại, không thuộc phòng ban đã chọn "
                    + "hoặc không có vai trò Staff.");
            }

            staffName = staff.FullName;
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        report.DepartmentId = request.DepartmentId;
        report.AssignedStaffId = request.StaffId;
        report.Status = ReportStatus.Assigned;
        report.RequiresManualAssignment = false;
        report.UpdatedAt = currentTime;

        var targetName = staffName is null
            ? department.Name
            : $"{staffName} - {department.Name}";

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Assigned,

                Note = string.IsNullOrWhiteSpace(request.Note)
                    ? $"Admin đã phân công báo cáo cho {targetName}."
                    : request.Note.Trim(),

                CreatedAt = currentTime
            });

        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.ReportAssigned,
            entityType: AuditEntityTypes.Report,
            entityId: report.Id.ToString(),
            detail: new
            {
                OldStatus = oldStatus,
                NewStatus = report.Status,

                report.DepartmentId,
                DepartmentName = department.Name,

                report.AssignedStaffId,
                AssignedStaffName = staffName,

                report.RequiresManualAssignment,

                Note = string.IsNullOrWhiteSpace(request.Note)
                    ? null
                    : request.Note.Trim()
            });

        /*
         * Report, StatusUpdate và AuditLog được lưu
         * chung trong một transaction của SaveChangesAsync.
         */
        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AdminReportActionResult(
            ReportId: report.Id,
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
