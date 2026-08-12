using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Admin.GetAdminReports;

public sealed class GetAdminReportsQueryValidator
    : AbstractValidator<GetAdminReportsQuery>
{
    public GetAdminReportsQueryValidator()
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

        RuleFor(query => query.CategoryId)
            .GreaterThan(0)
            .WithMessage("ID loại sự cố phải lớn hơn 0.")
            .When(query => query.CategoryId.HasValue);

        RuleFor(query => query.AreaId)
            .GreaterThan(0)
            .WithMessage("ID khu vực phải lớn hơn 0.")
            .When(query => query.AreaId.HasValue);

        RuleFor(query => query.DepartmentId)
            .GreaterThan(0)
            .WithMessage("ID phòng ban phải lớn hơn 0.")
            .When(query => query.DepartmentId.HasValue);

        RuleFor(query => query.StaffId)
            .NotEmpty()
            .WithMessage("ID Staff không hợp lệ.")
            .When(query => query.StaffId.HasValue);

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Số phần tử trên trang phải từ 1 đến 100.");
    }
}
