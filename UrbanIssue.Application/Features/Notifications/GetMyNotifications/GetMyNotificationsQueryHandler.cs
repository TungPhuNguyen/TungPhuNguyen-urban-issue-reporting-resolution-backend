using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Notifications.Common;

namespace UrbanIssue.Application.Features.Notifications.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler
    : IRequestHandler<
        GetMyNotificationsQuery,
        PagedResult<NotificationResult>>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public GetMyNotificationsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<NotificationResult>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId =
            _currentUserService.UserId;

        var query =
            _dbContext.Notifications
                .AsNoTracking()
                .Where(notification =>
                    notification.UserId == userId);

        if (request.IsRead.HasValue)
        {
            query =
                query.Where(notification =>
                    notification.IsRead
                        == request.IsRead.Value);
        }

        var totalItems =
            await query.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await query
                .OrderByDescending(notification =>
                    notification.CreatedAt)
                .ThenByDescending(notification =>
                    notification.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(notification =>
                    new NotificationResult(
                        notification.Id,
                        notification.ReportId,
                        notification.Type,
                        notification.Title,
                        notification.Message,
                        notification.IsRead,
                        notification.CreatedAt,
                        notification.ReadAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<NotificationResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
