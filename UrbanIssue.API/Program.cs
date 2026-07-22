using System.Text.Json.Serialization;
using UrbanIssue.API.Common.Exceptions;
using UrbanIssue.API.Extensions;
using UrbanIssue.API.OpenApi;
using UrbanIssue.Application;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Infrastructure.Sqlserver;

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

// Thêm CORS cho React frontend
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

// CORS phải chạy trước Authentication/Authorization
app.UseCors(
    AllowFrontendPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
