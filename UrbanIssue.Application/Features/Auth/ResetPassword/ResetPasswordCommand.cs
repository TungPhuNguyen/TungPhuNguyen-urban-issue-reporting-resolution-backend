using MediatR;

namespace UrbanIssue.Application.Features.Auth.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmNewPassword)
    : IRequest<bool>;
