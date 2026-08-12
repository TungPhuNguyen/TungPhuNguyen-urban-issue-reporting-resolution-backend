using MediatR;

namespace UrbanIssue.Application.Features.Auth.ResendVerificationEmail;

public sealed record ResendVerificationEmailCommand(string Email)
    : IRequest<bool>;
