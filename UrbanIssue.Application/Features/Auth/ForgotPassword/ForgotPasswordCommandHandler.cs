using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Email;
using UrbanIssue.Application.Common.Interfaces.Email;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Security;
using UrbanIssue.Application.Common.Settings;

namespace UrbanIssue.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _settings;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext dbContext,
        IEmailService emailService,
        IOptions<EmailSettings> settings)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _settings = settings.Value;
    }

    public async Task<bool> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            item => item.Email == normalizedEmail && item.IsActive,
            cancellationToken);

        // Luôn trả về cùng một kết quả để không làm lộ email đã đăng ký.
        if (user is null)
        {
            return true;
        }

        var token = OneTimeToken.Generate();
        user.PasswordResetTokenHash = OneTimeToken.Hash(token);
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(
            _settings.PasswordResetTokenLifetimeMinutes);
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var resetUrl = AuthActionUrlBuilder.Build(
            _settings.FrontendBaseUrl,
            "/reset-password",
            user.Email,
            token);
        await _emailService.SendPasswordResetAsync(
            user.Email,
            user.FullName,
            resetUrl,
            cancellationToken);

        return true;
    }
}
