using MediatR;

namespace UrbanIssue.Application.Features.Notifications.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand
    : IRequest<int>;
