using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Notifications.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler
    : IRequestHandler<
        MarkAllNotificationsAsReadCommand,
        int>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public MarkAllNotificationsAsReadCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId =
            _currentUserService.UserId;

        var currentTime =
            DateTime.UtcNow;

        var notifications =
            await _dbContext.Notifications
                .Where(notification =>
                    notification.UserId == userId
                    && !notification.IsRead)
                .ToListAsync(
                    cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead =
                true;

            notification.ReadAt =
                currentTime;
        }

        if (notifications.Count > 0)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return notifications.Count;
    }
}
