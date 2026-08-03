using MediatR;

namespace UrbanIssue.Application.Features.Notifications.GetUnreadNotificationCount;

public sealed record GetUnreadNotificationCountQuery()
    : IRequest<UnreadNotificationCountResult>;

public sealed record UnreadNotificationCountResult(int UnreadCount);
