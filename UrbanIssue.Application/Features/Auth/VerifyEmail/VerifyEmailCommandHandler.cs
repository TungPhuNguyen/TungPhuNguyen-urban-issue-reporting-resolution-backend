using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Security;

namespace UrbanIssue.Application.Features.Auth.VerifyEmail;

public sealed class VerifyEmailCommandHandler
    : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;

    public VerifyEmailCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            item => item.Email == normalizedEmail && item.IsActive,
            cancellationToken);

        if (user?.EmailVerifiedAt.HasValue == true)
        {
            return true;
        }

        if (user is null
            || !user.EmailVerificationTokenExpiresAt.HasValue
            || user.EmailVerificationTokenExpiresAt <= DateTime.UtcNow
            || !OneTimeToken.Matches(
                request.Token,
                user.EmailVerificationTokenHash ?? string.Empty))
        {
            throw new ConflictException(
                "Liên kết xác minh email không hợp lệ hoặc đã hết hạn.");
        }

        user.EmailVerifiedAt = DateTime.UtcNow;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
