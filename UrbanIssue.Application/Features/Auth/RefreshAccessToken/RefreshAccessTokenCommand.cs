using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.RefreshAccessToken
{
    public sealed record RefreshAccessTokenCommand(
    string RefreshToken)
    : IRequest<RefreshAccessTokenResult>;
}
