using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.Logout
{
    public sealed record LogoutCommand(
    string RefreshToken)
    : IRequest<bool>;
}
