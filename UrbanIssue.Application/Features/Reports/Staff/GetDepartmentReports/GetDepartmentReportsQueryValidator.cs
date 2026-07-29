using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.GetDepartmentReports;

public sealed class GetDepartmentReportsQueryValidator
    : AbstractValidator<GetDepartmentReportsQuery>
{
    public GetDepartmentReportsQueryValidator()
    {
        RuleFor(query => query.Search)
            .MaximumLength(200)
            .WithMessage("Từ khóa không được vượt quá 200 ký tự.")
            .When(query =>
                !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.Status)
            .IsInEnum()
            .WithMessage("Trạng thái báo cáo không hợp lệ.")
            .When(query => query.Status.HasValue);

        RuleFor(query => query.Priority)
            .IsInEnum()
            .WithMessage("Mức độ ưu tiên không hợp lệ.")
            .When(query => query.Priority.HasValue);

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Số phần tử trên trang phải từ 1 đến 100.");
    }
}
