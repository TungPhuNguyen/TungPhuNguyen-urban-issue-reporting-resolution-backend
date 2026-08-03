namespace UrbanIssue.API.Contracts.Auth;

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword);
