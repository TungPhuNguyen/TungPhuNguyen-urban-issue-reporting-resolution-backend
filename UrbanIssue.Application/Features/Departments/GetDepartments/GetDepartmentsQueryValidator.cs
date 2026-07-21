using FluentValidation;

namespace UrbanIssue.Application.Features.Departments.GetDepartments;

public sealed class GetDepartmentsQueryValidator
    : AbstractValidator<GetDepartmentsQuery>
{
    public GetDepartmentsQueryValidator()
    {
        RuleFor(query => query.Search)
            .MaximumLength(150)
            .WithMessage(
                "Từ khóa tìm kiếm không được vượt quá 150 ký tự.")
            .When(query =>
                !string.IsNullOrWhiteSpace(
                    query.Search));

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
