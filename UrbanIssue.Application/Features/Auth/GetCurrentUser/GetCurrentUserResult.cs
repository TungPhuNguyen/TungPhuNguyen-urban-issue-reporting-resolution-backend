using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.GetCurrentUser
{
    public sealed record GetCurrentUserResult(
    Guid UserId,
    string FullName,
    string Email,
    bool IsEmailVerified,
    string? PhoneNumber,
    string Role,
    int? DepartmentId,
    string? DepartmentName);
}
