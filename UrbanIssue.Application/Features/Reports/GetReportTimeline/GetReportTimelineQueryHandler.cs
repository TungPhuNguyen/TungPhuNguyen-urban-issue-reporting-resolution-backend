using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Reports.GetReportTimeline;

public sealed class GetReportTimelineQueryHandler
    : IRequestHandler<
        GetReportTimelineQuery,
        GetReportTimelineResult>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

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
        var citizenId =
            _currentUserService.UserId;

        /*
         * Kiểm tra đồng thời ReportId và CitizenId.
         *
         * Citizen không sở hữu Report cũng nhận 404,
         * tránh làm lộ Report có tồn tại hay không.
         */
        var report =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.Id == request.ReportId
                    && report.CitizenId == citizenId)
                .Select(report =>
                    new
                    {
                        report.Id,
                        report.Status
                    })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        var timelineItems =
            await _dbContext.StatusUpdates
                .AsNoTracking()
                .Where(statusUpdate =>
                    statusUpdate.ReportId
                        == request.ReportId)
                .OrderBy(statusUpdate =>
                    statusUpdate.CreatedAt)
                .ThenBy(statusUpdate =>
                    statusUpdate.Id)
                .Select(statusUpdate =>
                    new ReportTimelineItemResult(
                        statusUpdate.Id,
                        statusUpdate.OldStatus,
                        statusUpdate.NewStatus,
                        statusUpdate.Note,
                        statusUpdate.CreatedAt,

                        statusUpdate.Images
                            .OrderBy(image => image.Id)
                            .Select(image =>
                                image.ImageUrl)
                            .ToList()))
                .ToListAsync(
                    cancellationToken);

        return new GetReportTimelineResult(
            ReportId: report.Id,
            CurrentStatus: report.Status,
            Items: timelineItems);
    }
}
