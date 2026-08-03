namespace UrbanIssue.Application.Common.Interfaces.Notifications;

public interface INotificationRealtimePublisher
{
    Task PublishAsync(
        IReadOnlyList<NotificationRealtimeMessage> notifications,
        CancellationToken cancellationToken);
}

public sealed record NotificationRealtimeMessage(
    int Id,
    Guid UserId,
    Guid? ReportId,
    string Type,
    string Title,
    string Message,
    DateTime CreatedAt);
