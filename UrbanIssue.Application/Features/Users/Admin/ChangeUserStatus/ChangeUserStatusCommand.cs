using MediatR;
using UrbanIssue.Application.Features.Users.Admin.Common;

namespace UrbanIssue.Application.Features.Users.Admin.ChangeUserStatus;

public sealed record ChangeUserStatusCommand(
    Guid UserId,
    bool IsActive,
    string Reason)
    : IRequest<UserStatusActionResult>;
