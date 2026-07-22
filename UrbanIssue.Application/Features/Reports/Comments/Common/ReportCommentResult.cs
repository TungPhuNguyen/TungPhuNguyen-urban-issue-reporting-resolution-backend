namespace UrbanIssue.Application.Features.Reports.Comments.Common;

public sealed record ReportCommentResult(
    int Id,
    Guid ReportId,
    string AuthorName,
    string Content,
    bool IsMine,
    DateTime CreatedAt);
