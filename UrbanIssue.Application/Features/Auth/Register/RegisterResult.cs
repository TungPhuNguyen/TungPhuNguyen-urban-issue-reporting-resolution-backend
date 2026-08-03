using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.Register
{
    public sealed record RegisterResult(
    Guid UserId,
    string FullName,
    string Email,
    bool IsEmailVerified,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
}
