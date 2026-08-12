namespace UrbanIssue.Application.Features.Users.Admin.Common;

public sealed record AdminUserSummaryResult(
    Guid Id,
    string FullName,
    string Email,
    string RoleName,
    int? DepartmentId,
    string? DepartmentName,
    bool IsActive,
    int ActiveAssignedReportCount,
    int OverdueReportCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AdminUserDetailResult(
    Guid Id,
    string FullName,
    string Email,
    string RoleName,

    int? DepartmentId,
    string? DepartmentName,

    bool IsActive,

    int CreatedReportCount,
    int AssignedReportCount,
    int ActiveAssignedReportCount,

    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record StaffActionResult(
    Guid Id,
    string FullName,
    string Email,

    int DepartmentId,
    string DepartmentName,

    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record UserStatusActionResult(
    Guid Id,
    string FullName,
    string Email,
    string RoleName,
    bool IsActive,
    DateTime? UpdatedAt);
