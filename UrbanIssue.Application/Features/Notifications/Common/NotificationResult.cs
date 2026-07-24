using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Notifications.Common;

public sealed record NotificationResult(
    int Id,
    Guid? ReportId,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);
