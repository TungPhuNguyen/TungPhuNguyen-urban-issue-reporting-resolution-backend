using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Upvotes.Common;

namespace UrbanIssue.Application.Features.Reports.Upvotes.RemoveReportUpvote;

public sealed class RemoveReportUpvoteCommandHandler
    : IRequestHandler<
        RemoveReportUpvoteCommand,
        ReportUpvoteResult>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public RemoveReportUpvoteCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ReportUpvoteResult> Handle(
        RemoveReportUpvoteCommand request,
        CancellationToken cancellationToken)
    {
        var citizenId =
            _currentUserService.UserId;

        var report =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(
                    report =>
                        report.Id == request.ReportId)
                .Select(report => new
                {
                    report.ReportCode
                })
                .SingleOrDefaultAsync(cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        var upvote =
            await _dbContext.Upvotes
                .SingleOrDefaultAsync(
                    upvote =>
                        upvote.ReportId == request.ReportId
                        && upvote.UserId == citizenId,
                    cancellationToken);

        /*
         * DELETE cũng idempotent:
         * nếu chưa upvote thì không báo lỗi.
         */
        if (upvote is not null)
        {
            _dbContext.Upvotes.Remove(upvote);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var upvoteCount =
            await _dbContext.Upvotes
                .AsNoTracking()
                .CountAsync(
                    upvote =>
                        upvote.ReportId
                            == request.ReportId,
                    cancellationToken);

        return new ReportUpvoteResult(
            ReportId: request.ReportId,
            ReportCode: report.ReportCode,
            IsUpvoted: false,
            UpvoteCount: upvoteCount);
    }
}
