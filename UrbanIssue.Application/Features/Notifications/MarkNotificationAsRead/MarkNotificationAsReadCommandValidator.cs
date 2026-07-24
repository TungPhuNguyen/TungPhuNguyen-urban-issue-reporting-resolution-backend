using FluentValidation;

namespace UrbanIssue.Application.Features.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandValidator
    : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .GreaterThan(0)
            .WithMessage(
                "ID thông báo không hợp lệ.");
    }
}
