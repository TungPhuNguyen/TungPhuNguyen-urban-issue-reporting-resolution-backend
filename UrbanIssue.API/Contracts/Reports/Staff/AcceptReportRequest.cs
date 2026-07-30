using UrbanIssue.Domain.Enums;

namespace UrbanIssue.API.Contracts.Reports.Staff;

public sealed record AcceptReportRequest(
    ReportPriority Priority,
    string? Note);
