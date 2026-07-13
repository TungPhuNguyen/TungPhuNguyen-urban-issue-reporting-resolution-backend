using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.RefreshAccessToken
{
    public sealed record RefreshAccessTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
}
