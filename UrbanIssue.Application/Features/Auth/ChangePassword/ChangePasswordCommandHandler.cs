using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            item => item.Id == _currentUserService.UserId && item.IsActive,
            cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("Không tìm thấy tài khoản.");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ConflictException("Mật khẩu hiện tại không chính xác.");
        }

        var currentTime = DateTime.UtcNow;
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = currentTime;

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == user.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in activeRefreshTokens)
        {
            token.RevokedAt = currentTime;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
