using System.Text.Json.Serialization;
using UrbanIssue.API.BackgroundServices;
using UrbanIssue.API.Common.Exceptions;
using UrbanIssue.API.Extensions;
using UrbanIssue.API.OpenApi;
using UrbanIssue.API.Services;
using UrbanIssue.Application;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Storage;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Infrastructure.Sqlserver;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Infrastructure.Sqlserver.Services.Auditing;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Infrastructure.Sqlserver.Services.Notifications;
using UrbanIssue.Infrastructure.Sqlserver.Persistence.Seed;




var builder =
    WebApplication.CreateBuilder(args);

const string AllowFrontendPolicy =
    "AllowFrontend";

builder.Services
    .AddControllers()
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

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

builder.Services.AddScoped<
    IFileStorageService,
    LocalFileStorageService>();

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

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            AllowFrontendPolicy,
            policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

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

// Có thể bật lại khi HTTPS đã được cấu hình.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors(AllowFrontendPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
