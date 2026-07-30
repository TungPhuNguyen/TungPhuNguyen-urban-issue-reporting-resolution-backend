namespace UrbanIssue.Application.Features.Reports.Upvotes.Common;

public sealed record ReportUpvoteResult(
    Guid ReportId,
    bool IsUpvoted,
    int UpvoteCount);
