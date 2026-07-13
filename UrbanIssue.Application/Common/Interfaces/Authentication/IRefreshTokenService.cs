using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Application.Common.Models.Authentication;

namespace UrbanIssue.Application.Common.Interfaces.Authentication
{
    public interface IRefreshTokenService
    {
        RefreshTokenResult Generate();

        string Hash(string refreshToken);
    }
}
