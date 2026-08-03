using Microsoft.AspNetCore.SignalR;
using UrbanIssue.Application.Common.Interfaces.Notifications;

namespace UrbanIssue.API.Services;

public sealed class SignalRNotificationRealtimePublisher
    : INotificationRealtimePublisher
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly ILogger<SignalRNotificationRealtimePublisher> _logger;

    public SignalRNotificationRealtimePublisher(
        IHubContext<NotificationsHub> hubContext,
        ILogger<SignalRNotificationRealtimePublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(
        IReadOnlyList<NotificationRealtimeMessage> notifications,
        CancellationToken cancellationToken)
    {
        foreach (var notification in notifications)
        {
            try
            {
                await _hubContext.Clients
                    .User(notification.UserId.ToString())
                    .SendAsync(
                        "NotificationCreated",
                        notification,
                        cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Không thể đẩy realtime notification {NotificationId}.",
                    notification.Id);
            }
        }
    }
}
