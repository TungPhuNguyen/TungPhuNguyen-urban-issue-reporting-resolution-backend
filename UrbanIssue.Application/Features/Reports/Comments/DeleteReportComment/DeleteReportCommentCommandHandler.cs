using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Reports.Comments.DeleteReportComment;

public sealed class DeleteReportCommentCommandHandler
    : IRequestHandler<DeleteReportCommentCommand>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public DeleteReportCommentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(
        DeleteReportCommentCommand request,
        CancellationToken cancellationToken)
    {
        var userId =
            _currentUserService.UserId;

        /*
         * Lọc cả UserId để Citizen không thể xóa
         * bình luận của người khác.
         */
        var comment =
            await _dbContext.Comments
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == request.CommentId
                        && item.ReportId == request.ReportId
                        && item.UserId == userId,
                    cancellationToken);

        if (comment is null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy bình luận hoặc bạn không có quyền xóa bình luận này.");
        }

        _dbContext.Comments.Remove(comment);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
