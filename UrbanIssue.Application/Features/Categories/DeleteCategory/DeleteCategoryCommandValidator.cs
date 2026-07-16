using FluentValidation;

namespace UrbanIssue.Application.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryCommandValidator
    : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.");
    }
}
