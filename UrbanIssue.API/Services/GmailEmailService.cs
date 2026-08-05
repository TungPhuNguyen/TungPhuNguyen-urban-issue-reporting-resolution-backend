using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UrbanIssue.Application.Common.Interfaces.Email;
using UrbanIssue.Application.Common.Settings;

namespace UrbanIssue.API.Services;

public sealed class GmailEmailService
    : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<GmailEmailService> _logger;

    public GmailEmailService(
        IOptions<EmailSettings> settings,
        ILogger<GmailEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(
        string recipientEmail,
        string recipientName,
        string verificationUrl,
        CancellationToken cancellationToken)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeUrl = WebUtility.HtmlEncode(verificationUrl);

        var html =
            $$"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#f4f7fb;font-family:Arial,sans-serif;color:#172033">
              <div style="max-width:600px;margin:32px auto;background:#ffffff;border-radius:18px;padding:32px;border:1px solid #e7ebf2">
                <div style="font-size:22px;font-weight:700;color:#175cd3">CivicPulse</div>
                <h2 style="margin:24px 0 12px">Chào {{safeName}},</h2>
                <p style="line-height:1.65">Bạn vừa đăng ký tài khoản công dân trên CivicPulse.</p>
                <p style="line-height:1.65">Nhấn nút bên dưới để xác minh Gmail và hoàn tất tài khoản.</p>
                <p style="margin:28px 0">
                  <a href="{{safeUrl}}" style="display:inline-block;background:#175cd3;color:#ffffff;text-decoration:none;padding:13px 22px;border-radius:10px;font-weight:700">
                    Xác minh email
                  </a>
                </p>
                <p style="font-size:14px;color:#667085;line-height:1.6">
                  Liên kết có hiệu lực trong {{_settings.VerificationTokenLifetimeHours}} giờ.
                  Nếu bạn không tạo tài khoản, hãy bỏ qua email này.
                </p>
                <p style="font-size:12px;color:#98a2b3;word-break:break-all">{{safeUrl}}</p>
              </div>
            </body>
            </html>
            """;

        var text =
            $"Chào {recipientName},\n\n"
            + "Bạn vừa đăng ký tài khoản CivicPulse.\n"
            + $"Xác minh email tại: {verificationUrl}\n\n"
            + $"Liên kết có hiệu lực trong "
            + $"{_settings.VerificationTokenLifetimeHours} giờ.";

        return SendAsync(
            recipientEmail,
            "Xác minh tài khoản CivicPulse",
            html,
            text,
            cancellationToken);
    }

    public Task SendPasswordResetAsync(
        string recipientEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);

        var html =
            $$"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#f4f7fb;font-family:Arial,sans-serif;color:#172033">
              <div style="max-width:600px;margin:32px auto;background:#ffffff;border-radius:18px;padding:32px;border:1px solid #e7ebf2">
                <div style="font-size:22px;font-weight:700;color:#175cd3">CivicPulse</div>
                <h2 style="margin:24px 0 12px">Chào {{safeName}},</h2>
                <p style="line-height:1.65">Bạn vừa yêu cầu đặt lại mật khẩu CivicPulse.</p>
                <p style="margin:28px 0">
                  <a href="{{safeUrl}}" style="display:inline-block;background:#175cd3;color:#ffffff;text-decoration:none;padding:13px 22px;border-radius:10px;font-weight:700">
                    Đặt lại mật khẩu
                  </a>
                </p>
                <p style="font-size:14px;color:#667085;line-height:1.6">
                  Liên kết có hiệu lực trong {{_settings.PasswordResetTokenLifetimeMinutes}} phút.
                  Nếu bạn không gửi yêu cầu này, hãy bỏ qua email.
                </p>
                <p style="font-size:12px;color:#98a2b3;word-break:break-all">{{safeUrl}}</p>
              </div>
            </body>
            </html>
            """;

        var text =
            $"Chào {recipientName},\n\n"
            + $"Đặt lại mật khẩu tại: {resetUrl}\n\n"
            + $"Liên kết có hiệu lực trong "
            + $"{_settings.PasswordResetTokenLifetimeMinutes} phút.";

        return SendAsync(
            recipientEmail,
            "Đặt lại mật khẩu CivicPulse",
            html,
            text,
            cancellationToken);
    }

    private async Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        string textBody,
        CancellationToken cancellationToken)
    {
        var smtpUsername = _settings.SmtpUsername.Trim();
        var appPassword =
            _settings.SmtpAppPassword.Replace(" ", string.Empty);

        var message = new MimeMessage();
        message.From.Add(
            new MailboxAddress(
                _settings.FromName,
                smtpUsername));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = subject;
        message.Body =
            new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = textBody
            }.ToMessageBody();

        using var smtpClient = new SmtpClient
        {
            Timeout = 30_000
        };

        try
        {
            await smtpClient.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);

            smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");

            await smtpClient.AuthenticateAsync(
                smtpUsername,
                appPassword,
                cancellationToken);

            await smtpClient.SendAsync(
                message,
                cancellationToken);

            await smtpClient.DisconnectAsync(
                true,
                cancellationToken);

            _logger.LogInformation(
                "Đã gửi email '{Subject}' tới {RecipientEmail}.",
                subject,
                recipientEmail);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Không thể gửi email Gmail SMTP tới {RecipientEmail}.",
                recipientEmail);

            throw new InvalidOperationException(
                "Không thể gửi email xác minh. "
                + "Hãy kiểm tra cấu hình Gmail SMTP và App Password.",
                exception);
        }
    }
}
