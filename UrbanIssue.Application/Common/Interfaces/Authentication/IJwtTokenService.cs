using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Common.Interfaces.Authentication
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(
            Guid userId,
            string email,
            string fullName,
            string roleName);
    }
}
