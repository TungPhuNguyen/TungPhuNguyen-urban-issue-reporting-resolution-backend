using System.Text.Json;
using System.Text.Json.Serialization;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Services.Auditing;

public sealed class AuditLogService
    : IAuditLogService
{
    private static readonly JsonSerializerOptions
        SerializerOptions =
            CreateSerializerOptions();

    private readonly IApplicationDbContext _dbContext;

    public AuditLogService(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(
        Guid? userId,
        string action,
        string entityType,
        string entityId,
        object? detail = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);

        var serializedDetail =
            detail is null
                ? "{}"
                : JsonSerializer.Serialize(
                    detail,
                    SerializerOptions);

        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId =
                    userId,

                Action =
                    action,

                EntityType =
                    entityType,

                EntityId =
                    entityId,

                Detail =
                    serializedDetail,

                CreatedAt =
                    DateTime.UtcNow
            });
    }

    private static JsonSerializerOptions
        CreateSerializerOptions()
    {
        var options =
            new JsonSerializerOptions(
                JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull
            };

        options.Converters.Add(
            new JsonStringEnumConverter());

        return options;
    }
}
