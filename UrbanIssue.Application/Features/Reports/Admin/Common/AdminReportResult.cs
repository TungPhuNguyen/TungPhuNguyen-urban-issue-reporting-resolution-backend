using UrbanIssue.Domain.Enums;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.Admin.Common;

public sealed record AdminReportSummaryResult(
    Guid Id,
    string ReportCode,
    string Title,
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
    long ReportNumber,
    string ReportCode,
    string Title,

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
    string? OtherCategoryText,
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
    string? ReopenReason,

    ComplaintResult? Complaint,
    ReportResolutionResult? Resolution,
    ReportAllowedActionsResult AllowedActions,
    byte[] RowVersion);

public sealed record AdminReportActionResult(
    Guid ReportId,
    string ReportCode,
    ReportStatus Status,
    int? DepartmentId,
    string? DepartmentName,
    Guid? AssignedStaffId,
    string? AssignedStaffName,
    ReportPriority? Priority,
    bool RequiresManualAssignment,
    DateTime? UpdatedAt);
