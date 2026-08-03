using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Notifications.Common;

namespace UrbanIssue.Application.Features.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler
    : IRequestHandler<
        MarkNotificationAsReadCommand,
        NotificationResult>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public MarkNotificationAsReadCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<NotificationResult> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId =
            _currentUserService.UserId;

        var notification =
            await _dbContext.Notifications
                .Include(item => item.Report)
                .SingleOrDefaultAsync(
                    item =>
                        item.Id
                            == request.NotificationId
                        && item.UserId
                            == userId,
                    cancellationToken);

        if (notification is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy thông báo có ID "
                + $"{request.NotificationId}.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead =
                true;

            notification.ReadAt =
                DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return new NotificationResult(
            notification.Id,
            notification.ReportId,
            notification.Report?.ReportCode,
            notification.Report is null
                ? null
                : $"/reports/{notification.Report.ReportCode}",
            notification.Type,
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAt,
            notification.ReadAt);
    }
}
