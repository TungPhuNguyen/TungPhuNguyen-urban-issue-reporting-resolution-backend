using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Email;
using UrbanIssue.Application.Common.Interfaces.Email;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Security;
using UrbanIssue.Application.Common.Settings;

namespace UrbanIssue.Application.Features.Auth.ResendVerificationEmail;

public sealed class ResendVerificationEmailCommandHandler
    : IRequestHandler<ResendVerificationEmailCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _settings;

    public ResendVerificationEmailCommandHandler(
        IApplicationDbContext dbContext,
        IEmailService emailService,
        IOptions<EmailSettings> settings)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _settings = settings.Value;
    }

    public async Task<bool> Handle(
        ResendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            item => item.Email == normalizedEmail && item.IsActive,
            cancellationToken);

        if (user is null || user.EmailVerifiedAt.HasValue)
        {
            return true;
        }

        var token = OneTimeToken.Generate();
        user.EmailVerificationTokenHash = OneTimeToken.Hash(token);
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(
            _settings.VerificationTokenLifetimeHours);
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var verificationUrl = AuthActionUrlBuilder.Build(
            _settings.FrontendBaseUrl,
            "/verify-email",
            user.Email,
            token);
        await _emailService.SendEmailVerificationAsync(
            user.Email,
            user.FullName,
            verificationUrl,
            cancellationToken);

        return true;
    }
}
