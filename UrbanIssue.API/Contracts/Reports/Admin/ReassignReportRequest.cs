namespace UrbanIssue.API.Contracts.Reports.Admin;

public sealed record ReassignReportRequest(
    int DepartmentId,
    Guid? StaffId,
    string Reason);
