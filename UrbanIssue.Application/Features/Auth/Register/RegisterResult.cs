namespace UrbanIssue.Application.Features.Auth.Register;

public sealed record RegisterResult(
    Guid UserId,
    string FullName,
    string Email,
    bool IsEmailVerified,
    bool RequiresEmailVerification,
    string Role,
    string? AccessToken,
    string? RefreshToken,
    DateTime? RefreshTokenExpiresAt);
