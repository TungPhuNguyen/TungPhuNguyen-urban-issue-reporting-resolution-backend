using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.PostResolution.CloseReport;

public sealed class CloseReportCommandHandler
    : IRequestHandler<
        CloseReportCommand,
        PostResolutionActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CloseReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PostResolutionActionResult> Handle(
        CloseReportCommand request,
        CancellationToken cancellationToken)
    {
        var citizenId = _currentUserService.UserId;

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item =>
                    item.Id == request.ReportId
                    && item.CitizenId == citizenId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status != ReportStatus.Resolved)
        {
            throw new ConflictException(
                "Chỉ có thể đóng báo cáo đang ở trạng thái Resolved.");
        }

        if (report.ComplaintSubmittedAt.HasValue)
        {
            throw new ConflictException(
                "Không thể đóng báo cáo đang có khiếu nại chờ Admin xem xét.");
        }

        var currentTime = DateTime.UtcNow;
        var oldStatus = report.Status;

        report.Status = ReportStatus.Closed;
        report.ClosedAt = currentTime;
        report.UpdatedAt = currentTime;

        _dbContext.StatusUpdates.Add(
            new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = citizenId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Closed,
                Note = string.IsNullOrWhiteSpace(request.Note)
                    ? "Citizen đã xác nhận kết quả xử lý và đóng báo cáo."
                    : request.Note.Trim(),
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
                report.ResolvedAt?.AddDays(7),
            ClosedAt:
                report.ClosedAt,
            ReopenedAt:
                report.ReopenedAt,
            DueAt:
                report.DueAt);
    }
}
