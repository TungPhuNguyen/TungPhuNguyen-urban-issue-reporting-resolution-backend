using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Infrastructure.Sqlserver.Settings;

namespace UrbanIssue.Infrastructure.Sqlserver.Services.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(
        IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAccessToken(
        Guid userId,
        string email,
        string fullName,
        string roleName)
    {
        var now = DateTime.UtcNow;

        var expiresAt = now.AddMinutes(
            _jwtSettings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new(
                ClaimTypes.Email,
                email),

            new(
                ClaimTypes.Name,
                fullName),

            new(
                ClaimTypes.Role,
                roleName),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var secretKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _jwtSettings.SecretKey));

        var signingCredentials = new SigningCredentials(
            secretKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
