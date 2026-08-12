namespace UrbanIssue.Application.Features.Reports.Upvotes.Common;

public sealed record ReportUpvoteResult(
    Guid ReportId,
    string ReportCode,
    bool IsUpvoted,
    int UpvoteCount);
