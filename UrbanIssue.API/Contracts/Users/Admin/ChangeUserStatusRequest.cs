namespace UrbanIssue.API.Contracts.Users.Admin;

public sealed record ChangeUserStatusRequest(
    bool IsActive,
    string Reason);
