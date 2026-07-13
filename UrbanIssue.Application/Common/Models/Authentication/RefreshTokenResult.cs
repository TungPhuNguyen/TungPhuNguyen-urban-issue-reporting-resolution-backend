using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Common.Models.Authentication
{
    public sealed record RefreshTokenResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAt);
}
