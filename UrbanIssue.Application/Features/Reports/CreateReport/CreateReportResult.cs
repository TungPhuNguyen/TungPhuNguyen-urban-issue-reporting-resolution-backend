using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.CreateReport;

public sealed record CreateReportResult(
    Guid Id,
    long ReportNumber,
    string ReportCode,
    string Title,
    ReportStatus Status,
    int? DepartmentId,
    string? DepartmentName,
    bool RequiresManualAssignment,
    DateTime CreatedAt,
    IReadOnlyList<string> ImageUrls);
