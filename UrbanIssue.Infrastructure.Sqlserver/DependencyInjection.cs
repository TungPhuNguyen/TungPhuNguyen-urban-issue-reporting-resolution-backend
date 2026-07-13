using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Infrastructure.Sqlserver.Persistence;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Infrastructure.Sqlserver.Services.Authentication;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Infrastructure.Sqlserver.Services.Authentication;
using UrbanIssue.Infrastructure.Sqlserver.Settings;
using UrbanIssue.Application.Common.Interfaces.Persistence;


namespace UrbanIssue.Infrastructure.Sqlserver
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found.");

            services.AddDbContext<ApplicationDbContext>(
                options =>
                {
                    options.UseSqlServer(connectionString);
                });
            services.AddScoped<IApplicationDbContext>(
    serviceProvider =>
        serviceProvider
            .GetRequiredService<ApplicationDbContext>());
            services.AddScoped<
    IPasswordHasher,
    PasswordHasher>();
            services
    .AddOptions<JwtSettings>()
    .Bind(
        configuration.GetSection(
            JwtSettings.SectionName))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.Issuer),
        "JWT Issuer is required.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.Audience),
        "JWT Audience is required.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(
                settings.SecretKey),
        "JWT SecretKey is required.")
    .Validate(
        settings =>
            settings
                .AccessTokenExpirationMinutes > 0,
        "Access token expiration must be greater than zero.")
    .Validate(
        settings =>
            settings
                .RefreshTokenExpirationDays > 0,
        "Refresh token expiration days must be greater than zero.")
    .ValidateOnStart();

            services.AddScoped<
                IJwtTokenService,
                JwtTokenService>();
            services.AddScoped<
    IRefreshTokenService,
    RefreshTokenService>();

            return services;
        }
    }
}
