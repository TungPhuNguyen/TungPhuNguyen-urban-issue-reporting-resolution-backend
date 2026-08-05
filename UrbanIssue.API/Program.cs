using System.Text.Json.Serialization;
using UrbanIssue.API.BackgroundServices;
using UrbanIssue.API.Common.Exceptions;
using UrbanIssue.API.Extensions;
using UrbanIssue.API.OpenApi;
using UrbanIssue.API.Services;
using UrbanIssue.Application;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Email;
using UrbanIssue.Application.Common.Interfaces.Storage;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Infrastructure.Sqlserver;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Infrastructure.Sqlserver.Services.Auditing;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Infrastructure.Sqlserver.Services.Notifications;
using UrbanIssue.Infrastructure.Sqlserver.Persistence.Seed;
using UrbanIssue.API.Settings;
using System.Threading.RateLimiting;
using UrbanIssue.API.Common.ModelBinding;



var builder =
    WebApplication.CreateBuilder(args);

const string AllowFrontendPolicy =
    "AllowFrontend";

builder.Services
    .AddControllers(
        options =>
        {
            options.ModelBinderProviders.Insert(
                0,
                new InvariantDecimalModelBinderProvider());
        })
    .AddJsonOptions(
        options =>
        {
            options
                .JsonSerializerOptions
                .Converters
                .Add(
                    new JsonStringEnumConverter());
        });

builder.Services.AddProblemDetails();

builder.Services
    .AddExceptionHandler<
        GlobalExceptionHandler>();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddJwtAuthentication(
    builder.Configuration);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            AllowFrontendPolicy,
            policy =>
            {
                policy
                    .WithOrigins(
                        allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

builder.Services.AddOpenApi(
    options =>
    {
        options
            .AddDocumentTransformer<
                BearerSecuritySchemeTransformer>();

        options
            .AddOperationTransformer<
                BearerSecurityOperationTransformer>();
    });
builder.Services
    .AddOptions<DefaultSlaSettings>()
    .Bind(
        builder.Configuration.GetRequiredSection(
            DefaultSlaSettings.SectionName))
    .Validate(
        settings =>
            settings.LowHours > 0
            && settings.MediumHours > 0
            && settings.HighHours > 0,
        "Thời gian SLA mặc định phải lớn hơn 0.")
    .ValidateOnStart();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddOptions<LocationValidationSettings>()
    .Bind(builder.Configuration.GetRequiredSection(
        LocationValidationSettings.SectionName));
builder.Services.AddSignalR();
builder.Services.AddSingleton<
    INotificationRealtimePublisher,
    SignalRNotificationRealtimePublisher>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

builder.Services
    .AddOptions<CloudinarySettings>()
    .Bind(builder.Configuration.GetSection(CloudinarySettings.SectionName));
builder.Services.AddScoped<LocalFileStorageService>();
builder.Services
    .AddOptions<CloudinarySettings>()
    .Bind(
        builder.Configuration.GetRequiredSection(
            CloudinarySettings.SectionName))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.CloudName)
            && !string.IsNullOrWhiteSpace(settings.ApiKey)
            && !string.IsNullOrWhiteSpace(settings.ApiSecret),
        "Cloudinary phải có CloudName, ApiKey và ApiSecret.")
    .ValidateOnStart();

builder.Services.AddScoped<LocalFileStorageService>();
builder.Services.AddScoped<CloudinaryFileStorageService>();

builder.Services.AddScoped<IFileStorageService>(
    serviceProvider =>
    {
        var provider =
            builder.Configuration["FileStorage:Provider"]
            ?? "Local";

        return provider.Equals(
            "Cloudinary",
            StringComparison.OrdinalIgnoreCase)
            ? serviceProvider.GetRequiredService<
                CloudinaryFileStorageService>()
            : serviceProvider.GetRequiredService<
                LocalFileStorageService>();
    });

builder.Services
    .AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetRequiredSection(EmailSettings.SectionName))
    .Validate(
        settings => Uri.TryCreate(
            settings.FrontendBaseUrl,
            UriKind.Absolute,
            out _),
        "Email FrontendBaseUrl phải là URL tuyệt đối.")
    .Validate(
        settings => settings.VerificationTokenLifetimeHours > 0
            && settings.PasswordResetTokenLifetimeMinutes > 0,
        "Thời hạn token email phải lớn hơn 0.")
    .Validate(
        settings =>
            !settings.Provider.Equals(
                "Gmail",
                StringComparison.OrdinalIgnoreCase)
            || (
                !string.IsNullOrWhiteSpace(settings.SmtpHost)
                && settings.SmtpPort > 0
                && !string.IsNullOrWhiteSpace(settings.SmtpUsername)
                && settings.SmtpAppPassword
                    .Replace(" ", string.Empty)
                    .Length >= 16),
        "Gmail SMTP phải có SmtpHost, SmtpPort, "
        + "SmtpUsername và SmtpAppPassword hợp lệ.")
    .ValidateOnStart();

builder.Services.AddScoped<DevelopmentEmailService>();
builder.Services.AddScoped<GmailEmailService>();
builder.Services.AddHttpClient<ResendEmailService>();

builder.Services.AddScoped<IEmailService>(
    serviceProvider =>
    {
        var provider =
            builder.Configuration["Email:Provider"]
            ?? "Development";

        if (provider.Equals(
                "Gmail",
                StringComparison.OrdinalIgnoreCase))
        {
            return serviceProvider
                .GetRequiredService<GmailEmailService>();
        }

        if (provider.Equals(
                "Resend",
                StringComparison.OrdinalIgnoreCase))
        {
            return serviceProvider
                .GetRequiredService<ResendEmailService>();
        }

        return serviceProvider
            .GetRequiredService<DevelopmentEmailService>();
    });

builder.Services.AddHostedService<
    AutoCloseResolvedReportsBackgroundService>();

builder.Services.AddHostedService<
    SlaMonitoringBackgroundService>();

builder.Services.AddScoped<
    IAuditLogService,
    AuditLogService>();

builder.Services.AddScoped<
    INotificationService,
    NotificationService>();

var app =
    builder.Build();

if (app.Environment.IsDevelopment())
{
    using var seedScope =
        app.Services.CreateScope();

    var seeder =
        seedScope.ServiceProvider
            .GetRequiredService<
                HanoiDevelopmentDataSeeder>();

    await seeder.SeedAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "Urban Issue API v1");
        });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseCors(AllowFrontendPolicy);

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
