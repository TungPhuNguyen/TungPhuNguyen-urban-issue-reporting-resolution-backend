using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Common;

public sealed record CitizenReportSummaryResult(
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
    string? OtherCategoryText,
    string? AddressText,
    ReportPriority? Priority,
    ReportStatus Status,
    bool RequiresManualAssignment,
    int UpvoteCount,
    string? ThumbnailUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
