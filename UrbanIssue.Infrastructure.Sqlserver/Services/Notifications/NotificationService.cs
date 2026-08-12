using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Infrastructure.Sqlserver.Services.Notifications;

public sealed class NotificationService
    : INotificationService
{
    private readonly IApplicationDbContext _dbContext;

    public NotificationService(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(
        Guid userId,
        Guid? reportId,
        NotificationType type,
        string title,
        string message,
        DateTime createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID không hợp lệ.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _dbContext.Notifications.Add(
            new Notification
            {
                UserId = userId,
                ReportId = reportId,
                Type = type,
                Title = title.Trim(),
                Message = message.Trim(),
                IsRead = false,
                CreatedAt = createdAt,
                ReadAt = null
            });
    }

    public void AddMany(
        IEnumerable<Guid> userIds,
        Guid? reportId,
        NotificationType type,
        string title,
        string message,
        DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var distinctUserIds = userIds
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToList();

        foreach (var userId in distinctUserIds)
        {
            Add(
                userId: userId,
                reportId: reportId,
                type: type,
                title: title,
                message: message,
                createdAt: createdAt);
        }
    }
}
