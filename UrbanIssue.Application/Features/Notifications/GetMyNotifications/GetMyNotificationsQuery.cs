using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Notifications.Common;

namespace UrbanIssue.Application.Features.Notifications.GetMyNotifications;

public sealed record GetMyNotificationsQuery(
    bool? IsRead = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<NotificationResult>>;
