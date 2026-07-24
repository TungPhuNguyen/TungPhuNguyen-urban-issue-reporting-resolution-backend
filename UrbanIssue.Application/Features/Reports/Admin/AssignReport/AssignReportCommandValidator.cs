using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Admin.AssignReport;

public sealed class AssignReportCommandValidator
    : AbstractValidator<AssignReportCommand>
{
    public AssignReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .WithMessage("ID phòng ban phải lớn hơn 0.");

        RuleFor(x => x.StaffId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID Staff không hợp lệ.")
            .When(x => x.StaffId.HasValue);

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Ghi chú không được vượt quá 1000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
