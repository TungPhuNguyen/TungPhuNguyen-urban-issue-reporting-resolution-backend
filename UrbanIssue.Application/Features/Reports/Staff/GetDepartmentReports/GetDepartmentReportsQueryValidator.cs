using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.GetDepartmentReports;

public sealed class GetDepartmentReportsQueryValidator
    : AbstractValidator<GetDepartmentReportsQuery>
{
    public GetDepartmentReportsQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .WithMessage("Từ khóa không được vượt quá 200 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Trạng thái báo cáo không hợp lệ.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Mức độ ưu tiên không hợp lệ.")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Số phần tử trên trang phải từ 1 đến 100.");
    }
}
