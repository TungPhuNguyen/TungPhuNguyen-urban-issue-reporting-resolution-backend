using FluentValidation;

namespace UrbanIssue.Application.Features.Categories.GetCategories;

public sealed class GetCategoriesQueryValidator
    : AbstractValidator<GetCategoriesQuery>
{
    public GetCategoriesQueryValidator()
    {
        RuleFor(query => query.Search)
            .MaximumLength(100)
            .WithMessage(
                "Từ khóa tìm kiếm không được vượt quá 100 ký tự.")
            .When(
                query =>
                    !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Số phần tử trên mỗi trang phải nằm trong khoảng từ 1 đến 100.");
    }
}
