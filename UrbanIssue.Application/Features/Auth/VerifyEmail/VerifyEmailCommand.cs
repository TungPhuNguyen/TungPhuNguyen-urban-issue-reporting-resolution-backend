using MediatR;

namespace UrbanIssue.Application.Features.Auth.VerifyEmail;

public sealed record VerifyEmailCommand(string Email, string Token)
    : IRequest<bool>;
