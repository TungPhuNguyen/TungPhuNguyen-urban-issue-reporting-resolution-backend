using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.GetCurrentUser
{
    public sealed record GetCurrentUserQuery(
    Guid UserId)
    : IRequest<GetCurrentUserResult>;
}
