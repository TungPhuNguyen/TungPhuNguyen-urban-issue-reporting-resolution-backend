using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.Login
{
    public sealed record LoginResult(
    Guid UserId,
    string FullName,
    string Email,
    bool IsEmailVerified,
    string Role,
    int? DepartmentId,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
}
