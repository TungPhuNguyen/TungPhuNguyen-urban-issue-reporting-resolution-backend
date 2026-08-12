namespace UrbanIssue.API.Contracts.Reports;

public sealed record CancelReportRequest(
    string Reason,
    byte[] RowVersion);
