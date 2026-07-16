using FluentValidation;

namespace UrbanIssue.Application.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdQueryValidator
    : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.");
    }
}
