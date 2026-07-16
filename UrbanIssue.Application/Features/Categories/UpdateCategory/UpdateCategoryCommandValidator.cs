using FluentValidation;

namespace UrbanIssue.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandValidator
    : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.");

        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Tên loại sự cố không được để trống.")
            .MaximumLength(100)
            .WithMessage(
                "Tên loại sự cố không được vượt quá 100 ký tự.");

        RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithMessage(
                "Mô tả không được vượt quá 2000 ký tự.")
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.Description));
    }
}
