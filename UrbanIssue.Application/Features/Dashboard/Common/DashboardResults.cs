using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Dashboard.Common;

public sealed record DashboardSummaryResult(
    DateOnly FromDate,
    DateOnly ToDate,

    int TotalReports,

    int NewReports,
    int AssignedReports,
    int AcceptedReports,
    int InProgressReports,
    int ResolvedReports,
    int ClosedReports,
    int RejectedReports,

    int RequiresManualAssignmentReports,
    int PendingComplaintReports,
    int ActiveOverdueReports,
    int EscalatedReports,

    decimal ResolutionRate);

public sealed record ReportsByStatusResult(
    ReportStatus Status,
    int ReportCount,
    decimal Percentage);

public sealed record ReportsByCategoryResult(
    int CategoryId,
    string CategoryName,
    int ReportCount,
    decimal Percentage,
    double? AverageHandlingHours);

public sealed record ReportsByAreaResult(
    int AreaId,
    string AreaName,
    int ReportCount,
    decimal Percentage,
    double? AverageHandlingHours);

public sealed record SlaPerformanceResult(
    DateOnly FromDate,
    DateOnly ToDate,

    int SlaTrackedReports,
    int CompletedReports,
    int CompletedOnTimeReports,
    int CompletedLateReports,
    int ActiveOverdueReports,
    int EscalatedReports,

    decimal ComplianceRate,
    double? AverageHandlingHours);

public sealed record ReportTrendItemResult(
    DateOnly Date,
    int CreatedCount,
    int ResolvedCount,
    int ClosedCount);

public sealed record ReportTrendResult(
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyList<ReportTrendItemResult> Items);
