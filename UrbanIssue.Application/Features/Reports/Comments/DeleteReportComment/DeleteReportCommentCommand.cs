using MediatR;

namespace UrbanIssue.Application.Features.Reports.Comments.DeleteReportComment;

public sealed record DeleteReportCommentCommand(
    Guid ReportId,
    int CommentId)
    : IRequest;
