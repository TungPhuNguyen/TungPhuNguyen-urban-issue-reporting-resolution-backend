using MediatR;
using UrbanIssue.Application.Features.Notifications.Common;

namespace UrbanIssue.Application.Features.Notifications.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    int NotificationId)
    : IRequest<NotificationResult>;
