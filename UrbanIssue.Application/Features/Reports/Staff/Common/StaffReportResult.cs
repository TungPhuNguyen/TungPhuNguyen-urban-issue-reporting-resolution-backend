using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.Common;

public sealed record StaffReportSummaryResult(
    Guid Id,
    int CategoryId,
    string CategoryName,
    int AreaId,
    string AreaName,
    string Description,
    string? AddressText,
    ReportPriority? Priority,
    ReportStatus Status,
    Guid? AssignedStaffId,
    string? AssignedStaffName,
    bool RequiresManualAssignment,
    int UpvoteCount,
    string? ThumbnailUrl,
    DateTime CreatedAt,
    DateTime? DueAt,
    bool IsOverdue,
    double? OverdueHours,
    bool IsEscalated,
    DateTime? EscalatedAt);

public sealed record StaffReportDetailResult(
    Guid Id,
    Guid CitizenId,
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
    string? AddressText,
    decimal Latitude,
    decimal Longitude,

    ReportPriority? Priority,
    ReportStatus Status,

    int UpvoteCount,
    IReadOnlyList<string> ImageUrls,

    int? AppliedSlaHours,
    DateTime? SlaStartedAt,
    DateTime? DueAt,

    bool IsEscalated,
    DateTime? EscalatedAt,

    bool HasSubmittedComplaint,
    DateTime? ComplaintSubmittedAt,
    string? ComplaintReason,

    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? AcceptedAt,
    DateTime? ResolvedAt);

public sealed record StaffReportActionResult(
    Guid Id,
    ReportStatus Status,
    ReportPriority? Priority,
    Guid? AssignedStaffId,
    int? SlaConfigId,
    int? AppliedSlaHours,
    DateTime? SlaStartedAt,
    DateTime? DueAt,
    DateTime? UpdatedAt);
