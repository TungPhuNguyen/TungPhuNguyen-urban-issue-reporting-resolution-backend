namespace UrbanIssue.API.Contracts.Reports;

public sealed record CheckDuplicateReportsRequest(
    int CategoryId,
    decimal Latitude,
    decimal Longitude);
