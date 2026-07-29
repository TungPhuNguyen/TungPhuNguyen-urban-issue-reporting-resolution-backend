using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Admin.Common;

public sealed record AdminReportSummaryResult(
    Guid Id,
    string CitizenName,
    int CategoryId,
    string CategoryName,
    int AreaId,
    string AreaName,
    int? DepartmentId,
    string? DepartmentName,
    Guid? AssignedStaffId,
    string? AssignedStaffName,
    string Description,
    ReportPriority? Priority,
    ReportStatus Status,
    bool RequiresManualAssignment,
    bool HasComplaint,
    int UpvoteCount,
    string? ThumbnailUrl,
    DateTime CreatedAt,
    DateTime? DueAt,
    bool IsOverdue,
    double? OverdueHours,
    bool IsEscalated,
    DateTime? EscalatedAt);

public sealed record AdminReportDetailResult(
    Guid Id,

    Guid CitizenId,
    string CitizenName,
    string CitizenEmail,

    int CategoryId,
    string CategoryName,

    int AreaId,
    string AreaName,

    int? DepartmentId,
    string? DepartmentName,

    Guid? AssignedStaffId,
    string? AssignedStaffName,

    string Description,
    string? AddressText,
    decimal Latitude,
    decimal Longitude,

    ReportPriority? Priority,
    ReportStatus Status,
    bool RequiresManualAssignment,

    int UpvoteCount,
    int CommentCount,
    IReadOnlyList<string> ImageUrls,

    int? SlaConfigId,
    int? AppliedSlaHours,
    DateTime? SlaStartedAt,
    DateTime? DueAt,

    bool IsEscalated,
    DateTime? EscalatedAt,

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
    string? ReopenReason);

public sealed record AdminReportActionResult(
    Guid ReportId,
    ReportStatus Status,
    int? DepartmentId,
    string? DepartmentName,
    Guid? AssignedStaffId,
    string? AssignedStaffName,
    ReportPriority? Priority,
    bool RequiresManualAssignment,
    DateTime? UpdatedAt);
