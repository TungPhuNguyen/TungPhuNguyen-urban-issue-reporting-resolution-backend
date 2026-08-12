using MediatR;

namespace UrbanIssue.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email)
    : IRequest<bool>;
