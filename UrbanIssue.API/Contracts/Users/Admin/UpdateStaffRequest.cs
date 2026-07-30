namespace UrbanIssue.API.Contracts.Users.Admin;

public sealed record UpdateStaffRequest(
    string FullName,
    string Email,
    int DepartmentId);
