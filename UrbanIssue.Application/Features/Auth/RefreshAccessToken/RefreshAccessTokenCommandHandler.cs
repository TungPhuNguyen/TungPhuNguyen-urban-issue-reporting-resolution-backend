using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Settings;

using RefreshTokenEntity =
    UrbanIssue.Domain.Entities.RefreshToken;

namespace UrbanIssue.Application.Features.Auth.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler
    : IRequestHandler<
        RefreshAccessTokenCommand,
        RefreshAccessTokenResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly EmailSettings _emailSettings;

    public RefreshAccessTokenCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<EmailSettings> emailSettings)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _emailSettings = emailSettings.Value;
    }

    public async Task<RefreshAccessTokenResult> Handle(
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshTokenHash =
            _refreshTokenService.Hash(
                request.RefreshToken.Trim());

        var storedRefreshToken =
            await _dbContext.RefreshTokens
                .Include(refreshToken => refreshToken.User)
                .ThenInclude(user => user.Role)
                .SingleOrDefaultAsync(
                    refreshToken =>
                        refreshToken.TokenHash == refreshTokenHash,
                    cancellationToken);

        if (storedRefreshToken is null)
        {
            throw new UnauthorizedAccessException(
                "Refresh token không hợp lệ hoặc đã hết hạn.");
        }

        var currentTime = DateTime.UtcNow;

        if (storedRefreshToken.RevokedAt is not null
            || storedRefreshToken.ExpiresAt <= currentTime)
        {
            throw new UnauthorizedAccessException(
                "Refresh token không hợp lệ hoặc đã hết hạn.");
        }

        var user = storedRefreshToken.User;

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Tài khoản đã bị vô hiệu hóa.");
        }

        if (_emailSettings.RequireVerification
            && !user.EmailVerifiedAt.HasValue)
        {
            storedRefreshToken.RevokedAt = currentTime;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            throw new UnauthorizedAccessException(
                "Bạn cần xác minh email trước khi tiếp tục sử dụng tài khoản.");
        }

        storedRefreshToken.RevokedAt = currentTime;

        var generatedRefreshToken =
            _refreshTokenService.Generate();

        var newRefreshToken =
            new RefreshTokenEntity
            {
                UserId = user.Id,
                TokenHash = generatedRefreshToken.TokenHash,
                ExpiresAt = generatedRefreshToken.ExpiresAt,
                RevokedAt = null,
                CreatedAt = currentTime
            };

        _dbContext.RefreshTokens.Add(newRefreshToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var newAccessToken =
            _jwtTokenService.GenerateAccessToken(
                userId: user.Id,
                email: user.Email,
                fullName: user.FullName,
                roleName: user.Role.Name);

        return new RefreshAccessTokenResult(
            AccessToken: newAccessToken,
            RefreshToken: generatedRefreshToken.Token,
            RefreshTokenExpiresAt: generatedRefreshToken.ExpiresAt);
    }
}
