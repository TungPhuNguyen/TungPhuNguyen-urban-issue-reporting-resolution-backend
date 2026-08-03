using MediatR;

namespace UrbanIssue.Application.Features.Auth.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword)
    : IRequest<bool>;
