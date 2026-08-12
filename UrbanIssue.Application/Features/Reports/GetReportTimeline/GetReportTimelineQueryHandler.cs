using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.GetReportTimeline;

public sealed class GetReportTimelineQueryHandler
    : IRequestHandler<
        GetReportTimelineQuery,
        GetReportTimelineResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetReportTimelineQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<GetReportTimelineResult> Handle(
        GetReportTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var currentRole = _currentUserService.Role;

        if (string.IsNullOrWhiteSpace(currentRole))
        {
            throw new UnauthorizedAccessException(
                "Access token không chứa Role hợp lệ.");
        }

        var report = await _dbContext.Reports
            .AsNoTracking()
            .Where(item => item.Id == request.ReportId)
            .Select(item => new TimelineReportAccess(
                item.Id,
                item.ReportCode,
                item.Title,
                item.Status,
                item.CitizenId,
                item.DepartmentId,
                item.AssignedStaffId))
            .SingleOrDefaultAsync(cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        var canView = currentRole switch
        {
            "Admin" => true,
            "Citizen" => report.CitizenId == currentUserId,
            "Staff" => await CanStaffViewAsync(
                report,
                currentUserId,
                cancellationToken),
            _ => false
        };

        /*
         * Trả 404 thay vì 403 để không làm lộ sự tồn tại
         * của Report cho tài khoản không có quyền xem.
         */
        if (!canView)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        var timelineItems = await _dbContext.StatusUpdates
            .AsNoTracking()
            .Where(statusUpdate =>
                statusUpdate.ReportId == request.ReportId)
            .OrderBy(statusUpdate => statusUpdate.CreatedAt)
            .ThenBy(statusUpdate => statusUpdate.Id)
            .Select(statusUpdate =>
                new ReportTimelineItemResult(
                    statusUpdate.Id,
                    statusUpdate.EventType,
                    statusUpdate.OldStatus,
                    statusUpdate.NewStatus,
                    statusUpdate.Note,
                    statusUpdate.UpdatedByUserId,
                    statusUpdate.UpdatedByUser == null
                        ? null
                        : statusUpdate.UpdatedByUser.FullName,
                    statusUpdate.CreatedAt,
                    statusUpdate.Images
                        .OrderBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .ToList()))
            .ToListAsync(cancellationToken);

        return new GetReportTimelineResult(
            ReportId: report.Id,
            ReportCode: report.ReportCode,
            Title: report.Title,
            CurrentStatus: report.Status,
            Items: timelineItems);
    }

    private async Task<bool> CanStaffViewAsync(
        TimelineReportAccess report,
        Guid staffId,
        CancellationToken cancellationToken)
    {
        var staff = await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Id == staffId
                && user.IsActive
                && user.Role.Name == "Staff")
            .Select(user => new
            {
                user.DepartmentId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (staff is null || !staff.DepartmentId.HasValue)
        {
            throw new ConflictException(
                "Tài khoản Staff chưa được gán phòng ban.");
        }

        return report.DepartmentId == staff.DepartmentId.Value
            && (
                report.Status == ReportStatus.Assigned
                || report.AssignedStaffId == staffId
            );
    }

    private sealed record TimelineReportAccess(
        Guid Id,
        string ReportCode,
        string Title,
        ReportStatus Status,
        Guid CitizenId,
        int? DepartmentId,
        Guid? AssignedStaffId);
}
