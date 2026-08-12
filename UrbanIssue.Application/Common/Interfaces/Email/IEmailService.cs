namespace UrbanIssue.Application.Common.Interfaces.Email;

public interface IEmailService
{
    Task SendEmailVerificationAsync(
        string recipientEmail,
        string recipientName,
        string verificationUrl,
        CancellationToken cancellationToken);

    Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken);
}
