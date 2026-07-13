using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UrbanIssue.Infrastructure.Sqlserver.Settings;

namespace UrbanIssue.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings =
                configuration
                    .GetRequiredSection(
                        JwtSettings.SectionName)
                    .Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "Không tìm thấy cấu hình JWT.");

            if (string.IsNullOrWhiteSpace(
                    jwtSettings.Issuer))
            {
                throw new InvalidOperationException(
                    "JWT Issuer không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(
                    jwtSettings.Audience))
            {
                throw new InvalidOperationException(
                    "JWT Audience không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(
                    jwtSettings.SecretKey))
            {
                throw new InvalidOperationException(
                    "JWT SecretKey không được để trống.");
            }

            if (Encoding.UTF8.GetByteCount(
                    jwtSettings.SecretKey) < 32)
            {
                throw new InvalidOperationException(
                    "JWT SecretKey phải có ít nhất 32 byte.");
            }

            services
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme =
                            JwtBearerDefaults
                                .AuthenticationScheme;

                        options.DefaultChallengeScheme =
                            JwtBearerDefaults
                                .AuthenticationScheme;

                        options.DefaultScheme =
                            JwtBearerDefaults
                                .AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        options.SaveToken = false;

                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuer = true,

                                ValidIssuer =
                                    jwtSettings.Issuer,

                                ValidateAudience = true,

                                ValidAudience =
                                    jwtSettings.Audience,

                                ValidateIssuerSigningKey =
                                    true,

                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(
                                            jwtSettings
                                                .SecretKey)),

                                ValidateLifetime = true,

                                ClockSkew =
                                    TimeSpan.Zero,

                                NameClaimType =
                                    ClaimTypes.Name,

                                RoleClaimType =
                                    ClaimTypes.Role
                            };
                    });

            services.AddAuthorization();

            return services;
        }
    }
}
