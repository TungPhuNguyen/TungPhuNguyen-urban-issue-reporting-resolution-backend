using FluentValidation;

namespace UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigs;

public sealed class GetSlaConfigsQueryValidator
    : AbstractValidator<GetSlaConfigsQuery>
{
    public GetSlaConfigsQueryValidator()
    {
        RuleFor(query => query.Search)
            .MaximumLength(100)
            .WithMessage(
                "Từ khóa tìm kiếm không được vượt quá 100 ký tự.")
            .When(query =>
                !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.CategoryId)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.")
            .When(query =>
                query.CategoryId.HasValue);

        RuleFor(query => query.Priority)
            .IsInEnum()
            .WithMessage(
                "Mức độ ưu tiên không hợp lệ.")
            .When(query =>
                query.Priority.HasValue);

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
