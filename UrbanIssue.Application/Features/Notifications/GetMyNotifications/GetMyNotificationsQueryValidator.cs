using FluentValidation;

namespace UrbanIssue.Application.Features.Notifications.GetMyNotifications;

public sealed class GetMyNotificationsQueryValidator
    : AbstractValidator<GetMyNotificationsQuery>
{
    public GetMyNotificationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Số phần tử trên trang phải từ 1 đến 100.");
    }
}
