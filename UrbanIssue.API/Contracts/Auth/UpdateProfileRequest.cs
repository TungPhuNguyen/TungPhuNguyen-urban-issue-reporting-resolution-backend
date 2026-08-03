namespace UrbanIssue.API.Contracts.Auth;

public sealed record UpdateProfileRequest(
    string FullName,
    string? PhoneNumber);
