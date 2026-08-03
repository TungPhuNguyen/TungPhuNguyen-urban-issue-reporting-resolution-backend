namespace UrbanIssue.Application.Common.Settings;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string Provider { get; init; } = "Development";

    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";

    public string FromAddress { get; init; } = "onboarding@resend.dev";

    public string FromName { get; init; } = "UrbanIssue";

    public string ResendApiKey { get; init; } = string.Empty;

    public int VerificationTokenLifetimeHours { get; init; } = 24;

    public int PasswordResetTokenLifetimeMinutes { get; init; } = 30;
}
