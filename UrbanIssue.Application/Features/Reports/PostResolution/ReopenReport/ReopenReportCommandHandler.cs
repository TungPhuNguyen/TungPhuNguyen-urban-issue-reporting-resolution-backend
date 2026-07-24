using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.PostResolution.ReopenReport;

public sealed class ReopenReportCommandHandler
    : IRequestHandler<
        ReopenReportCommand,
        PostResolutionActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ReopenReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PostResolutionActionResult> Handle(
        ReopenReportCommand request,
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

        if (report.Status != ReportStatus.Resolved)
        {
            throw new ConflictException(
                "Chỉ có thể mở lại báo cáo đang ở trạng thái Resolved.");
        }

        if (!report.ComplaintSubmittedAt.HasValue
            || string.IsNullOrWhiteSpace(
                report.ComplaintReason))
        {
            throw new ConflictException(
                "Báo cáo không có khiếu nại đang chờ xử lý.");
        }

        if (!report.AssignedStaffId.HasValue)
        {
            throw new ConflictException(
                "Báo cáo chưa có Staff phụ trách để tiếp tục xử lý.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;
        var complaintReason = report.ComplaintReason;
        var adminReason = request.Reason.Trim();

        report.Status = ReportStatus.InProgress;

        report.ReopenedAt = currentTime;
        report.ReopenedByUserId = adminId;
        report.ReopenReason = adminReason;

        /*
         * Xóa thời điểm Resolved hiện tại.
         * Khi Staff Resolve lại, ResolvedAt sẽ được ghi mới.
         */
        report.ResolvedAt = null;
        report.ClosedAt = null;

        /*
         * Khởi động lại SLA với snapshot đã áp dụng.
         */
        if (report.AppliedSLAHours.HasValue)
        {
            report.SLAStartedAt = currentTime;

            report.DueAt = currentTime.AddHours(
                report.AppliedSLAHours.Value);
        }

        report.SLAWarningSentAt = null;
        report.SLABreachedNotifiedAt = null;
        report.IsEscalated = false;
        report.EscalatedAt = null;

        /*
         * Khiếu nại đã được Admin xử lý.
         * Lịch sử vẫn được lưu trong StatusUpdate.
         */
        report.ComplaintSubmittedAt = null;
        report.ComplaintReason = null;

        report.UpdatedAt = currentTime;

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = adminId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.InProgress,
                Note =
                    $"Admin mở lại báo cáo. "
                    + $"Khiếu nại của Citizen: {complaintReason}. "
                    + $"Lý do mở lại: {adminReason}",
                CreatedAt = currentTime
            });

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new PostResolutionActionResult(
            ReportId: report.Id,
            Status: report.Status,
            ComplaintSubmittedAt:
                report.ComplaintSubmittedAt,
            ComplaintDeadline:
                null,
            ClosedAt:
                report.ClosedAt,
            ReopenedAt:
                report.ReopenedAt,
            DueAt:
                report.DueAt);
    }
}
