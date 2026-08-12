using UrbanIssue.Application.Common.Interfaces.Email;

namespace UrbanIssue.API.Services;

public sealed class DevelopmentEmailService
    : IEmailService
{
    private readonly ILogger<DevelopmentEmailService> _logger;

    public DevelopmentEmailService(
        ILogger<DevelopmentEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(
        string recipientEmail,
        string recipientName,
        string verificationUrl,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Xác minh email cho {RecipientName} <{RecipientEmail}>: {VerificationUrl}",
            recipientName,
            recipientEmail,
            verificationUrl);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Đặt lại mật khẩu cho {RecipientName} <{RecipientEmail}>: {ResetUrl}",
            recipientName,
            recipientEmail,
            resetUrl);
        return Task.CompletedTask;
    }
}
