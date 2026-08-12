using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Common;

public sealed record CitizenReportDetailResult(
    Guid Id,
    long ReportNumber,
    string ReportCode,
    string Title,

    int CategoryId,
    string CategoryName,

    int AreaId,
    string AreaName,

    int? DepartmentId,
    string? DepartmentName,

    string Description,
    string? OtherCategoryText,
    string? AddressText,

    decimal Latitude,
    decimal Longitude,

    ReportPriority? Priority,
    ReportStatus Status,

    bool RequiresManualAssignment,
    int UpvoteCount,

    IReadOnlyList<string> ImageUrls,

    int? AppliedSlaHours,
    DateTime? SlaStartedAt,
    DateTime? DueAt,

    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? AcceptedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,

    bool HasSubmittedComplaint,
    DateTime? ComplaintSubmittedAt,
    string? ComplaintReason,

    DateTime? RejectedAt,
    string? RejectedReason,

    DateTime? ReopenedAt,

    string? ReopenReason,

    bool IsUpvotedByCurrentUser,
    ComplaintResult? Complaint,
    ReportResolutionResult? Resolution,
    ReportAllowedActionsResult AllowedActions,
    byte[] RowVersion);
