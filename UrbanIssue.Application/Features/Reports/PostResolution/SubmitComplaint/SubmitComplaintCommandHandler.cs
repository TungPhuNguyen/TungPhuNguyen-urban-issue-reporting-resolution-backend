using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;

public sealed class SubmitComplaintCommandHandler
    : IRequestHandler<
        SubmitComplaintCommand,
        PostResolutionActionResult>
{
    private const int ComplaintPeriodInDays = 7;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public SubmitComplaintCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PostResolutionActionResult> Handle(
        SubmitComplaintCommand request,
        CancellationToken cancellationToken)
    {
        var citizenId = _currentUserService.UserId;

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item =>
                    item.Id == request.ReportId
                    && item.CitizenId == citizenId,
                cancellationToken);

        /*
         * Lọc theo cả ReportId và CitizenId để Citizen
         * không thể khiếu nại báo cáo của người khác.
         */
        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status != ReportStatus.Resolved)
        {
            throw new ConflictException(
                "Chỉ có thể khiếu nại báo cáo đang ở trạng thái Resolved.");
        }

        if (!report.ResolvedAt.HasValue)
        {
            throw new ConflictException(
                "Báo cáo chưa có thời điểm hoàn tất xử lý.");
        }

        if (report.ComplaintSubmittedAt.HasValue)
        {
            throw new ConflictException(
                "Báo cáo đã có khiếu nại đang chờ Admin xem xét.");
        }

        var currentTime = DateTime.UtcNow;

        var complaintDeadline =
            report.ResolvedAt.Value.AddDays(
                ComplaintPeriodInDays);

        if (currentTime > complaintDeadline)
        {
            throw new ConflictException(
                "Đã hết thời hạn 7 ngày để gửi khiếu nại.");
        }

        var reason = request.Reason.Trim();

        report.ComplaintSubmittedAt = currentTime;
        report.ComplaintReason = reason;
        report.UpdatedAt = currentTime;

        /*
         * Khiếu nại chưa làm thay đổi trạng thái.
         * Report vẫn là Resolved cho đến khi Admin Reopen.
         */
        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = citizenId,
                OldStatus = ReportStatus.Resolved,
                NewStatus = ReportStatus.Resolved,
                Note = $"Citizen đã gửi khiếu nại: {reason}",
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
                complaintDeadline,
            ClosedAt:
                report.ClosedAt,
            ReopenedAt:
                report.ReopenedAt,
            DueAt:
                report.DueAt);
    }
}
