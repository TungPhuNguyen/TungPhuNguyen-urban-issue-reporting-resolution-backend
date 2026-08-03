using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Upvotes.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Upvotes.AddReportUpvote;

public sealed class AddReportUpvoteCommandHandler
    : IRequestHandler<
        AddReportUpvoteCommand,
        ReportUpvoteResult>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public AddReportUpvoteCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ReportUpvoteResult> Handle(
        AddReportUpvoteCommand request,
        CancellationToken cancellationToken)
    {
        var citizenId =
            _currentUserService.UserId;

        var report =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.Id == request.ReportId)
                .Select(report => new
                {
                    report.Id,
                    report.ReportCode,
                    report.CitizenId,
                    report.Status
                })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.CitizenId == citizenId)
        {
            throw new ConflictException(
                "Bạn không thể upvote báo cáo do chính mình tạo.");
        }

        if (report.Status is
            ReportStatus.Closed or
            ReportStatus.Rejected or
            ReportStatus.Cancelled)
        {
            throw new ConflictException(
                "Không thể upvote báo cáo đã đóng hoặc bị từ chối.");
        }

        var alreadyUpvoted =
            await _dbContext.Upvotes
                .AnyAsync(
                    upvote =>
                        upvote.ReportId == request.ReportId
                        && upvote.UserId == citizenId,
                    cancellationToken);

        /*
         * POST được thiết kế idempotent:
         * nếu đã upvote thì không thêm bản ghi mới.
         */
        if (!alreadyUpvoted)
        {
            var upvote =
                new Upvote
                {
                    ReportId =
                        request.ReportId,

                    UserId =
                        citizenId,

                    CreatedAt =
                        DateTime.UtcNow
                };

            _dbContext.Upvotes.Add(upvote);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var upvoteCount =
            await _dbContext.Upvotes
                .CountAsync(
                    upvote =>
                        upvote.ReportId
                            == request.ReportId,
                    cancellationToken);

        return new ReportUpvoteResult(
            ReportId: request.ReportId,
            ReportCode: report.ReportCode,
            IsUpvoted: true,
            UpvoteCount: upvoteCount);
    }
}
