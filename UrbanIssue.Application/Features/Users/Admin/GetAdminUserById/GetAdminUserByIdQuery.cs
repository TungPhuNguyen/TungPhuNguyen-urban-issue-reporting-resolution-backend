using MediatR;
using UrbanIssue.Application.Features.Users.Admin.Common;

namespace UrbanIssue.Application.Features.Users.Admin.GetAdminUserById;

public sealed record GetAdminUserByIdQuery(
    Guid UserId)
    : IRequest<AdminUserDetailResult>;
