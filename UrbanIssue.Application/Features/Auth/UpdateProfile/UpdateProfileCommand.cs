using MediatR;
using UrbanIssue.Application.Features.Auth.GetCurrentUser;

namespace UrbanIssue.Application.Features.Auth.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FullName,
    string? PhoneNumber)
    : IRequest<GetCurrentUserResult>;
