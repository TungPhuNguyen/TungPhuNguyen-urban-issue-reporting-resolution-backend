namespace UrbanIssue.API.Contracts.Users.Admin;

public sealed record CreateStaffRequest(
    string FullName,
    string Email,
    string Password,
    int DepartmentId);
