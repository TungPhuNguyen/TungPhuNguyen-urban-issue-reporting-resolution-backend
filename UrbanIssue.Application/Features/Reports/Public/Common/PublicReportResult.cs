using UrbanIssue.Domain.Enums;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.Public.Common;

public sealed record PublicReportMapItemResult(
    Guid Id,
    string ReportCode,
    string Title,

    int CategoryId,
    string CategoryName,

    int AreaId,
    string AreaName,

    int? DepartmentId,
    string? DepartmentName,

    string Description,
    string? AddressText,

    decimal Latitude,
    decimal Longitude,

    ReportPriority? Priority,
    ReportStatus Status,

    int UpvoteCount,
    bool IsUpvotedByCurrentUser,
    int CommentCount,

    string? ThumbnailUrl,

    DateTime CreatedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    ReportAllowedActionsResult AllowedActions);

public sealed record PublicReportDetailResult(
    Guid Id,
    string ReportCode,
    string Title,

    int CategoryId,
    string CategoryName,

    int AreaId,
    string AreaName,

    int? DepartmentId,
    string? DepartmentName,

    string Description,
    string? AddressText,

    decimal Latitude,
    decimal Longitude,

    ReportPriority? Priority,
    ReportStatus Status,

    int UpvoteCount,
    bool IsUpvotedByCurrentUser,
    int CommentCount,

    IReadOnlyList<string> ImageUrls,

    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? AcceptedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    ReportAllowedActionsResult AllowedActions);
