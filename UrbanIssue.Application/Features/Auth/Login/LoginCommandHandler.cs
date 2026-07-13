using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Features.Auth.Login
{
    public sealed class LoginCommandHandler
    : IRequestHandler<
        LoginCommand,
        LoginResult>
    {
        private readonly IApplicationDbContext
            _dbContext;

        private readonly IPasswordHasher
            _passwordHasher;

        private readonly IJwtTokenService
            _jwtTokenService;

        private readonly IRefreshTokenService
            _refreshTokenService;

        public LoginCommandHandler(
            IApplicationDbContext dbContext,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService)
        {
            _dbContext =
                dbContext;

            _passwordHasher =
                passwordHasher;

            _jwtTokenService =
                jwtTokenService;

            _refreshTokenService =
                refreshTokenService;
        }

        public async Task<LoginResult> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var user =
                await _dbContext.Users
                    .AsNoTracking()
                    .Include(
                        user =>
                            user.Role)
                    .SingleOrDefaultAsync(
                        user =>
                            user.Email
                                == normalizedEmail,
                        cancellationToken);

            if (user is null)
            {
                throw new UnauthorizedAccessException(
                    "Email hoặc mật khẩu không chính xác.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Tài khoản đã bị vô hiệu hóa.");
            }

            var isPasswordValid =
                _passwordHasher.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException(
                    "Email hoặc mật khẩu không chính xác.");
            }

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
                            user.Role.Name);

            var generatedRefreshToken =
                _refreshTokenService
                    .Generate();

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
                        DateTime.UtcNow
                };

            _dbContext.RefreshTokens.Add(
                refreshToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return new LoginResult(
                UserId:
                    user.Id,

                FullName:
                    user.FullName,

                Email:
                    user.Email,

                Role:
                    user.Role.Name,

                DepartmentId:
                    user.DepartmentId,

                AccessToken:
                    accessToken,

                RefreshToken:
                    generatedRefreshToken.Token,

                RefreshTokenExpiresAt:
                    generatedRefreshToken.ExpiresAt);
        }
    }
}
