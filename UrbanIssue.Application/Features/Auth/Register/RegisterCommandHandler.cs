using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Email;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Email;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Security;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Domain.Constants;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Features.Auth.Register
{
    public sealed class RegisterCommandHandler
    : IRequestHandler<
        RegisterCommand,
        RegisterResult>
    {
        private readonly IApplicationDbContext _dbContext;

        private readonly IPasswordHasher _passwordHasher;

        private readonly IJwtTokenService _jwtTokenService;

        private readonly IRefreshTokenService
            _refreshTokenService;

        private readonly IEmailService _emailService;

        private readonly EmailSettings _emailSettings;

        public RegisterCommandHandler(
            IApplicationDbContext dbContext,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings)
        {
            _dbContext = dbContext;

            _passwordHasher = passwordHasher;

            _jwtTokenService = jwtTokenService;

            _refreshTokenService =
                refreshTokenService;

            _emailService = emailService;

            _emailSettings = emailSettings.Value;
        }

        public async Task<RegisterResult> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var emailAlreadyExists =
                await _dbContext.Users
                    .AnyAsync(
                        user =>
                            user.Email
                                == normalizedEmail,
                        cancellationToken);

            if (emailAlreadyExists)
            {
                throw new ConflictException(
                    "Email đã được sử dụng.");
            }

            var citizenRole =
                await _dbContext.Roles
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        role =>
                            role.Name
                                == RoleNames.Citizen,
                        cancellationToken);

            if (citizenRole is null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy Role Citizen. "
                    + "Hãy kiểm tra dữ liệu seed Role.");
            }

            var currentTime =
                DateTime.UtcNow;

            var verificationToken =
                OneTimeToken.Generate();

            var user = new User
            {
                Id = Guid.NewGuid(),

                FullName =
                    request.FullName.Trim(),

                Email =
                    normalizedEmail,

                PasswordHash =
                    _passwordHasher.Hash(
                        request.Password),

                PhoneNumber =
                    string.IsNullOrWhiteSpace(
                        request.PhoneNumber)
                        ? null
                        : request.PhoneNumber.Trim(),

                RoleId =
                    citizenRole.Id,

                DepartmentId =
                    null,

                IsActive =
                    true,

                EmailVerifiedAt =
                    null,

                EmailVerificationTokenHash =
                    OneTimeToken.Hash(verificationToken),

                EmailVerificationTokenExpiresAt =
                    currentTime.AddHours(
                        _emailSettings.VerificationTokenLifetimeHours),

                CreatedAt =
                    currentTime,

                UpdatedAt =
                    null
            };

            var generatedRefreshToken =
                _refreshTokenService.Generate();

            var refreshToken =
                new RefreshToken
                {
                    UserId =
                        user.Id,

                    TokenHash =
                        generatedRefreshToken
                            .TokenHash,

                    ExpiresAt =
                        generatedRefreshToken
                            .ExpiresAt,

                    RevokedAt =
                        null,

                    CreatedAt =
                        currentTime
                };

            _dbContext.Users.Add(
                user);

            _dbContext.RefreshTokens.Add(
                refreshToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            var verificationUrl =
                AuthActionUrlBuilder.Build(
                    _emailSettings.FrontendBaseUrl,
                    "/verify-email",
                    user.Email,
                    verificationToken);

            await _emailService.SendEmailVerificationAsync(
                user.Email,
                user.FullName,
                verificationUrl,
                cancellationToken);

            var accessToken =
                _jwtTokenService
                    .GenerateAccessToken(
                        userId:
                            user.Id,

                        email:
                            user.Email,

                        fullName:
                            user.FullName,

                        roleName:
                            citizenRole.Name);

            return new RegisterResult(
                UserId:
                    user.Id,

                FullName:
                    user.FullName,

                Email:
                    user.Email,

                IsEmailVerified:
                    false,

                Role:
                    citizenRole.Name,

                AccessToken:
                    accessToken,

                RefreshToken:
                    generatedRefreshToken.Token,

                RefreshTokenExpiresAt:
                    generatedRefreshToken.ExpiresAt);
        }
    }
}
