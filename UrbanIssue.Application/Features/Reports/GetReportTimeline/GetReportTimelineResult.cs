using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.GetReportTimeline;

public sealed record GetReportTimelineResult(
    Guid ReportId,
    ReportStatus CurrentStatus,
    IReadOnlyList<ReportTimelineItemResult> Items);

public sealed record ReportTimelineItemResult(
    long Id,
    ReportStatus? OldStatus,
    ReportStatus NewStatus,
    string? Note,
    DateTime CreatedAt,
    IReadOnlyList<string> ImageUrls);
