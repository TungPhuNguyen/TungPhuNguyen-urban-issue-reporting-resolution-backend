using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

public sealed record CheckDuplicateReportsResult(
    bool HasPossibleDuplicates,
    double SearchRadiusInMeters,
    IReadOnlyList<DuplicateReportResult> Reports);

public sealed record DuplicateReportResult(
    Guid Id,
    string ReportCode,
    string Title,
    string Description,
    decimal Latitude,
    decimal Longitude,
    double DistanceInMeters,
    ReportStatus Status,
    int UpvoteCount,
    string? ThumbnailUrl,
    DateTime CreatedAt);
