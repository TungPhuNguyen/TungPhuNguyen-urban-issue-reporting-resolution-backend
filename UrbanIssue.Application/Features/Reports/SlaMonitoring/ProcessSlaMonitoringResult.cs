namespace UrbanIssue.Application.Features.Reports.SlaMonitoring;

public sealed record ProcessSlaMonitoringResult(
    int WarningReportCount,
    int BreachedReportCount,
    int EscalatedReportCount,
    int CreatedNotificationCount);
