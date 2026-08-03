using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Security;

namespace UrbanIssue.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            item => item.Email == normalizedEmail && item.IsActive,
            cancellationToken);

        if (user is null
            || !user.PasswordResetTokenExpiresAt.HasValue
            || user.PasswordResetTokenExpiresAt <= DateTime.UtcNow
            || !OneTimeToken.Matches(
                request.Token,
                user.PasswordResetTokenHash ?? string.Empty))
        {
            throw new ConflictException(
                "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }

        var currentTime = DateTime.UtcNow;
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;
        user.UpdatedAt = currentTime;

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == user.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAt = currentTime;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
