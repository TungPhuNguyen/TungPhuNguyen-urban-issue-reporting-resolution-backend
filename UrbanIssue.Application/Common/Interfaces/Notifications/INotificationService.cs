using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Common.Interfaces.Notifications;

public interface INotificationService
{
    void Add(
        Guid userId,
        Guid? reportId,
        NotificationType type,
        string title,
        string message,
        DateTime createdAt);

    void AddMany(
        IEnumerable<Guid> userIds,
        Guid? reportId,
        NotificationType type,
        string title,
        string message,
        DateTime createdAt);
}
