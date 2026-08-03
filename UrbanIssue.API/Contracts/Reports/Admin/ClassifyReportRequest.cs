namespace UrbanIssue.API.Contracts.Reports.Admin;

public sealed record ClassifyReportRequest(
    int CategoryId,
    string? Note);
