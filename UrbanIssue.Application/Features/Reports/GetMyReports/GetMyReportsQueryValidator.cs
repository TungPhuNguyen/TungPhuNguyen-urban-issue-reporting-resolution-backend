using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.GetMyReports;

public sealed class GetMyReportsQueryValidator
    : AbstractValidator<GetMyReportsQuery>
{
    public GetMyReportsQueryValidator()
    {
        RuleFor(query => query.Search)
            .MaximumLength(200)
            .WithMessage(
                "Từ khóa tìm kiếm không được vượt quá 200 ký tự.")
            .When(query =>
                !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.Status)
            .IsInEnum()
            .WithMessage(
                "Trạng thái báo cáo không hợp lệ.")
            .When(query =>
                query.Status.HasValue);

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
