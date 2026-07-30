using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Public.Common;

public sealed record PublicReportMapItemResult(
    Guid Id,

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
    int CommentCount,

    string? ThumbnailUrl,

    DateTime CreatedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt);

public sealed record PublicReportDetailResult(
    Guid Id,

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
    int CommentCount,

    IReadOnlyList<string> ImageUrls,

    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? AcceptedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt);
