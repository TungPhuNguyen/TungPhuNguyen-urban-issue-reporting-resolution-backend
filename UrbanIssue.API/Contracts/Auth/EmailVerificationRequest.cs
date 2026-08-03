namespace UrbanIssue.API.Contracts.Auth;

public sealed record EmailVerificationRequest(
    string Email,
    string Token);
