namespace UrbanIssue.Application.Features.AuditLogs.Common;

public sealed record AuditLogSummaryResult(
    int Id,
    Guid? UserId,
    string? UserName,
    string? UserEmail,
    string Action,
    string EntityType,
    string EntityId,
    DateTime CreatedAt);

public sealed record AuditLogDetailResult(
    int Id,
    Guid? UserId,
    string? UserName,
    string? UserEmail,
    string Action,
    string EntityType,
    string EntityId,
    string Detail,
    DateTime CreatedAt);
