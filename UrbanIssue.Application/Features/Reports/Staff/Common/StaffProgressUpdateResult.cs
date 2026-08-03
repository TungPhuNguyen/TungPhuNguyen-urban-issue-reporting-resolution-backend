using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.Common;

public sealed record StaffProgressUpdateResult(
    Guid ReportId,
    string ReportCode,
    int StatusUpdateId,
    ReportStatus Status,
    string? Note,
    IReadOnlyList<string> ImageUrls,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
