using FluentValidation;

namespace UrbanIssue.Application.Features.Areas.UpdateArea;

public sealed class UpdateAreaCommandValidator
    : AbstractValidator<UpdateAreaCommand>
{
    public UpdateAreaCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực phải lớn hơn 0.");

        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Tên khu vực không được để trống.")
            .MaximumLength(100)
            .WithMessage(
                "Tên khu vực không được vượt quá 100 ký tự.");

        RuleFor(command => command.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Mã khu vực không được để trống.")
            .MaximumLength(20)
            .WithMessage(
                "Mã khu vực không được vượt quá 20 ký tự.")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage(
                "Mã khu vực chỉ được chứa chữ cái, chữ số, dấu gạch ngang và gạch dưới.");

        RuleFor(command => command.ParentAreaId)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực cha phải lớn hơn 0.")
            .When(command =>
                command.ParentAreaId.HasValue);
    }
}
