using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.PostResolution.Common;

public sealed record PostResolutionActionResult(
    Guid ReportId,
    ReportStatus Status,
    DateTime? ComplaintSubmittedAt,
    DateTime? ComplaintDeadline,
    DateTime? ClosedAt,
    DateTime? ReopenedAt,
    DateTime? DueAt);
