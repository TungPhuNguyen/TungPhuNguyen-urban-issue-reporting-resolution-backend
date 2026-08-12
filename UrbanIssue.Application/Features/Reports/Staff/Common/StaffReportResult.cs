using UrbanIssue.Domain.Enums;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.Staff.Common;

public sealed record StaffReportSummaryResult(
    Guid Id,
    string ReportCode,
    string Title,
    int CategoryId,
    string CategoryName,
    int AreaId,
    string AreaName,
    string Description,
    string? OtherCategoryText,
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
    long ReportNumber,
    string ReportCode,
    string Title,
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
    string? OtherCategoryText,
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
    DateTime? ResolvedAt,

    ComplaintResult? Complaint,
    ReportResolutionResult? Resolution,
    ReportAllowedActionsResult AllowedActions,
    byte[] RowVersion);

public sealed record StaffReportActionResult(
    Guid Id,
    string ReportCode,
    ReportStatus Status,
    ReportPriority? Priority,
    Guid? AssignedStaffId,
    int? SlaConfigId,
    int? AppliedSlaHours,
    DateTime? SlaStartedAt,
    DateTime? DueAt,
    DateTime? UpdatedAt);
