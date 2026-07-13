using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Models.Authentication;
using UrbanIssue.Infrastructure.Sqlserver.Settings;

namespace UrbanIssue.Infrastructure.Sqlserver.Services.Authentication
{
    public sealed class RefreshTokenService
    : IRefreshTokenService
    {
        private const int TokenSizeInBytes = 64;

        private readonly JwtSettings _jwtSettings;

        public RefreshTokenService(
            IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public RefreshTokenResult Generate()
        {
            var randomBytes =
                RandomNumberGenerator.GetBytes(
                    TokenSizeInBytes);

            var token =
                Base64UrlEncoder.Encode(
                    randomBytes);

            var tokenHash =
                Hash(token);

            var expiresAt =
                DateTime.UtcNow.AddDays(
                    _jwtSettings
                        .RefreshTokenExpirationDays);

            return new RefreshTokenResult(
                Token: token,
                TokenHash: tokenHash,
                ExpiresAt: expiresAt);
        }

        public string Hash(
            string refreshToken)
        {
            ArgumentException
                .ThrowIfNullOrWhiteSpace(
                    refreshToken);

            var tokenBytes =
                Encoding.UTF8.GetBytes(
                    refreshToken);

            var hashBytes =
                SHA256.HashData(
                    tokenBytes);

            return Convert.ToHexString(
                hashBytes);
        }
    }
}
