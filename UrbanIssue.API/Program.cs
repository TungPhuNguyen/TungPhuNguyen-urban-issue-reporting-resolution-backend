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




var builder =
    WebApplication.CreateBuilder(args);

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
