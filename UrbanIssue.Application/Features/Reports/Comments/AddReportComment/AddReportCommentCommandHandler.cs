using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Comments.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Comments.AddReportComment;

public sealed class AddReportCommentCommandHandler
    : IRequestHandler<
        AddReportCommentCommand,
        ReportCommentResult>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public AddReportCommentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ReportCommentResult> Handle(
        AddReportCommentCommand request,
        CancellationToken cancellationToken)
    {
        var userId =
            _currentUserService.UserId;

        var report =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.Id == request.ReportId)
                .Select(report => new
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

        if (report.Status is
            ReportStatus.Closed or
            ReportStatus.Rejected)
        {
            throw new ConflictException(
                "Không thể bình luận báo cáo đã đóng hoặc bị từ chối.");
        }

        var comment =
            new Comment
            {
                ReportId =
                    request.ReportId,

                UserId =
                    userId,

                Content =
                    request.Content.Trim(),

                CreatedAt =
                    DateTime.UtcNow
            };

        _dbContext.Comments.Add(comment);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var result =
            await _dbContext.Comments
                .AsNoTracking()
                .Where(item =>
                    item.Id == comment.Id)
                .Select(item =>
                    new ReportCommentResult(
                        item.Id,
                        item.ReportId,
                        item.User.FullName,
                        item.Content,
                        item.UserId == userId,
                        item.CreatedAt))
                .SingleAsync(
                    cancellationToken);

        return result;
    }
}
