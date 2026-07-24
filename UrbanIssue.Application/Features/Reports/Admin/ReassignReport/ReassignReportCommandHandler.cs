using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
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

    public ReassignReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<AdminReportActionResult> Handle(
        ReassignReportCommand request,
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

        if (report.Status is not (
            ReportStatus.Assigned
            or ReportStatus.Accepted
            or ReportStatus.InProgress))
        {
            throw new ConflictException(
                "Chỉ có thể phân công lại báo cáo đang ở trạng thái Assigned, Accepted hoặc InProgress.");
        }

        if (report.DepartmentId == request.DepartmentId
            && report.AssignedStaffId == request.StaffId)
        {
            throw new ConflictException(
                "Báo cáo đã được phân công đúng phòng ban và Staff này.");
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
            .SingleOrDefaultAsync(cancellationToken);

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
                .SingleOrDefaultAsync(cancellationToken);

            if (staff is null)
            {
                throw new ConflictException(
                    "Staff không tồn tại, không thuộc phòng ban đã chọn hoặc không có vai trò Staff.");
            }

            staffName = staff.FullName;
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        report.DepartmentId = request.DepartmentId;
        report.AssignedStaffId = request.StaffId;
        report.Status = ReportStatus.Assigned;
        report.RequiresManualAssignment = false;

        /*
         * Staff mới phải Accept lại để chọn Priority
         * và áp dụng SLA mới.
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

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Assigned,
                Note =
                    $"Admin phân công lại báo cáo cho {targetName}. "
                    + $"Lý do: {request.Reason.Trim()}",
                CreatedAt = currentTime
            });

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
