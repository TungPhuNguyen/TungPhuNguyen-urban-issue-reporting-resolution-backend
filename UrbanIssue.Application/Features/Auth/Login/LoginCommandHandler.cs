using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Features.Auth.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly EmailSettings _emailSettings;

    public LoginCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<EmailSettings> emailSettings)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _emailSettings = emailSettings.Value;
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
                .Include(user => user.Role)
                .SingleOrDefaultAsync(
                    user => user.Email == normalizedEmail,
                    cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Email hoặc mật khẩu không chính xác.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Tài khoản đã bị khóa.");
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

        if (_emailSettings.RequireVerification
            && !user.EmailVerifiedAt.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Bạn cần xác minh email trước khi đăng nhập. "
                + "Hãy kiểm tra hộp thư hoặc yêu cầu gửi lại email xác minh.");
        }

        var accessToken =
            _jwtTokenService.GenerateAccessToken(
                userId: user.Id,
                email: user.Email,
                fullName: user.FullName,
                roleName: user.Role.Name);

        var generatedRefreshToken =
            _refreshTokenService.Generate();

        var refreshToken =
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = generatedRefreshToken.TokenHash,
                ExpiresAt = generatedRefreshToken.ExpiresAt,
                RevokedAt = null,
                CreatedAt = DateTime.UtcNow
            };

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new LoginResult(
            UserId: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            IsEmailVerified: user.EmailVerifiedAt.HasValue,
            Role: user.Role.Name,
            DepartmentId: user.DepartmentId,
            AccessToken: accessToken,
            RefreshToken: generatedRefreshToken.Token,
            RefreshTokenExpiresAt: generatedRefreshToken.ExpiresAt);
    }
}
