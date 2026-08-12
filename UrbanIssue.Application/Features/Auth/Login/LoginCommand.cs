using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.Login
{
    public sealed record LoginCommand(
    string Email,
    string Password)
    : IRequest<LoginResult>;
}
