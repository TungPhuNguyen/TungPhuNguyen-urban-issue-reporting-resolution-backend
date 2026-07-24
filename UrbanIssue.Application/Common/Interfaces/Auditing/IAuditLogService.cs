namespace UrbanIssue.Application.Common.Interfaces.Auditing;

public interface IAuditLogService
{
    void Add(
        Guid? userId,
        string action,
        string entityType,
        string entityId,
        object? detail = null);
}
