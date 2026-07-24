using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
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

    public RejectReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<AdminReportActionResult> Handle(
        RejectReportCommand request,
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

        if (report.Status is
            ReportStatus.Resolved
            or ReportStatus.Closed
            or ReportStatus.Rejected)
        {
            throw new ConflictException(
                "Không thể từ chối báo cáo đã Resolved, Closed hoặc Rejected.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;
        var reason = request.Reason.Trim();

        report.Status = ReportStatus.Rejected;
        report.RejectedAt = currentTime;
        report.RejectedByUserId = adminId;
        report.RejectedReason = reason;
        report.RequiresManualAssignment = false;
        report.UpdatedAt = currentTime;

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Rejected,
                Note = $"Admin từ chối báo cáo. Lý do: {reason}",
                CreatedAt = currentTime
            });

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        string? departmentName = null;
        string? staffName = null;

        if (report.DepartmentId.HasValue)
        {
            departmentName = await _dbContext.Departments
                .AsNoTracking()
                .Where(department =>
                    department.Id == report.DepartmentId.Value)
                .Select(department =>
                    department.Name)
                .SingleOrDefaultAsync(cancellationToken);
        }

        if (report.AssignedStaffId.HasValue)
        {
            staffName = await _dbContext.Users
                .AsNoTracking()
                .Where(user =>
                    user.Id == report.AssignedStaffId.Value)
                .Select(user =>
                    user.FullName)
                .SingleOrDefaultAsync(cancellationToken);
        }

        return new AdminReportActionResult(
            ReportId: report.Id,
            Status: report.Status,
            DepartmentId: report.DepartmentId,
            DepartmentName: departmentName,
            AssignedStaffId: report.AssignedStaffId,
            AssignedStaffName: staffName,
            Priority: report.Priority,
            RequiresManualAssignment:
                report.RequiresManualAssignment,
            UpdatedAt: report.UpdatedAt);
    }
}
