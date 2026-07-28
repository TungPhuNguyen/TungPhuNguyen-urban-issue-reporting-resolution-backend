using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.Dashboard.Common;

public sealed record StaffDashboardResult(
    int DepartmentId,
    string DepartmentName,
    DateTime From,
    DateTime To,
    int TotalReports,
    int NewReports,
    int AssignedReports,
    int AcceptedReports,
    int InProgressReports,
    int ResolvedReports,
    int ClosedReports,
    int RejectedReports,
    int SlaWarningReports,
    int SlaBreachedReports,
    int EscalatedReports,
    int OverdueReports,
    double? AverageResolutionHours,
    IReadOnlyList<StaffDashboardStatusItem>
        ReportsByStatus,
    IReadOnlyList<StaffDashboardTrendItem>
        Trend);

public sealed record StaffDashboardStatusItem(
    ReportStatus Status,
    int Count);

public sealed record StaffDashboardTrendItem(
    DateTime Date,
    int Count);
