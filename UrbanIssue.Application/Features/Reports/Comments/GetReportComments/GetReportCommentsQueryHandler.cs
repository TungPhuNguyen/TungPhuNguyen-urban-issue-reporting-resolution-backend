using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Comments.Common;

namespace UrbanIssue.Application.Features.Reports.Comments.GetReportComments;

public sealed class GetReportCommentsQueryHandler
    : IRequestHandler<
        GetReportCommentsQuery,
        PagedResult<ReportCommentResult>>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public GetReportCommentsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<ReportCommentResult>>
        Handle(
            GetReportCommentsQuery request,
            CancellationToken cancellationToken)
    {
        var reportExists =
            await _dbContext.Reports
                .AsNoTracking()
                .AnyAsync(
                    report =>
                        report.Id == request.ReportId,
                    cancellationToken);

        if (!reportExists)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        var userId =
            _currentUserService.UserId;

        var commentsQuery =
            _dbContext.Comments
                .AsNoTracking()
                .Where(comment =>
                    comment.ReportId == request.ReportId);

        var totalItems =
            await commentsQuery.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await commentsQuery
                .OrderBy(comment =>
                    comment.CreatedAt)
                .ThenBy(comment =>
                    comment.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(comment =>
                    new ReportCommentResult(
                        comment.Id,
                        comment.ReportId,
                        comment.User.FullName,
                        comment.Content,
                        comment.UserId == userId,
                        comment.CreatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<ReportCommentResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
