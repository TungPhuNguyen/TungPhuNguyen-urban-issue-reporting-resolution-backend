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




var builder =
    WebApplication.CreateBuilder(args);

var allowedOrigins =
    builder.Configuration
        .GetSection("Frontend:AllowedOrigins")
        .Get<string[]>()
    ?? [];

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

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendPolicy",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app =
    builder.Build();

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

app.UseCors("FrontendPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
