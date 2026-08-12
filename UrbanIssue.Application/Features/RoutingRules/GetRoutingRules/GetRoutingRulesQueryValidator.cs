using FluentValidation;

namespace UrbanIssue.Application.Features.RoutingRules.GetRoutingRules;

public sealed class GetRoutingRulesQueryValidator
    : AbstractValidator<GetRoutingRulesQuery>
{
    public GetRoutingRulesQueryValidator()
    {
        RuleFor(query => query.Search)
            .MaximumLength(150)
            .WithMessage(
                "Từ khóa tìm kiếm không được vượt quá 150 ký tự.")
            .When(query =>
                !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.CategoryId)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.")
            .When(query =>
                query.CategoryId.HasValue);

        RuleFor(query => query.AreaId)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực phải lớn hơn 0.")
            .When(query =>
                query.AreaId.HasValue);

        RuleFor(query => query.DepartmentId)
            .GreaterThan(0)
            .WithMessage(
                "ID đơn vị xử lý phải lớn hơn 0.")
            .When(query =>
                query.DepartmentId.HasValue);

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
