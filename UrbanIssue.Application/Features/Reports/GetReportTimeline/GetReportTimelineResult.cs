using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.GetReportTimeline;

public sealed record GetReportTimelineResult(
    Guid ReportId,
    string ReportCode,
    string Title,
    ReportStatus CurrentStatus,
    IReadOnlyList<ReportTimelineItemResult> Items);

public sealed record ReportTimelineItemResult(
    int Id,
    TimelineEventType EventType,
    ReportStatus? OldStatus,
    ReportStatus NewStatus,
    string? Note,
    Guid? UpdatedByUserId,
    string? UpdatedByUserName,
    DateTime CreatedAt,
    IReadOnlyList<string> ImageUrls);
