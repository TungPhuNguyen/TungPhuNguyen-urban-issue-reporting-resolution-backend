namespace UrbanIssue.API.Contracts.Reports.Admin;

public sealed record AssignReportRequest(
    int DepartmentId,
    Guid? StaffId,
    string? Note);
