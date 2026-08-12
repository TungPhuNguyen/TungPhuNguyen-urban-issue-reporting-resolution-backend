using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Interfaces.Email;
using UrbanIssue.Application.Common.Settings;

namespace UrbanIssue.API.Services;

public sealed class ResendEmailService
    : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;

    public ResendEmailService(
        HttpClient httpClient,
        IOptions<EmailSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.ResendApiKey))
        {
            throw new InvalidOperationException(
                "Email:ResendApiKey chưa được cấu hình.");
        }
    }

    public Task SendEmailVerificationAsync(
        string recipientEmail,
        string recipientName,
        string verificationUrl,
        CancellationToken cancellationToken)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var safeUrl = WebUtility.HtmlEncode(verificationUrl);
        return SendAsync(
            recipientEmail,
            "Xác minh email UrbanIssue",
            $"""
            <h2>Chào {safeName},</h2>
            <p>Hãy xác minh email để hoàn tất tài khoản UrbanIssue.</p>
            <p><a href="{safeUrl}">Xác minh email</a></p>
            <p>Liên kết này sẽ hết hạn sau {_settings.VerificationTokenLifetimeHours} giờ.</p>
            """,
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
        return SendAsync(
            recipientEmail,
            "Đặt lại mật khẩu UrbanIssue",
            $"""
            <h2>Chào {safeName},</h2>
            <p>Bạn vừa yêu cầu đặt lại mật khẩu UrbanIssue.</p>
            <p><a href="{safeUrl}">Đặt lại mật khẩu</a></p>
            <p>Liên kết này sẽ hết hạn sau {_settings.PasswordResetTokenLifetimeMinutes} phút.</p>
            <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>
            """,
            cancellationToken);
    }

    private async Task SendAsync(
        string recipientEmail,
        string subject,
        string html,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _settings.ResendApiKey);
        request.Content = JsonContent.Create(new
        {
            from = $"{_settings.FromName} <{_settings.FromAddress}>",
            to = new[] { recipientEmail },
            subject,
            html
        });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Không thể gửi email qua Resend ({(int)response.StatusCode}): {responseBody}");
    }
}
