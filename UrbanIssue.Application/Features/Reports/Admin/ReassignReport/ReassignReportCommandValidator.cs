using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Admin.ReassignReport;

public sealed class ReassignReportCommandValidator
    : AbstractValidator<ReassignReportCommand>
{
    public ReassignReportCommandValidator()
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

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Lý do phân công lại không được để trống.")
            .MinimumLength(5)
            .WithMessage("Lý do phân công lại phải có ít nhất 5 ký tự.")
            .MaximumLength(1000)
            .WithMessage("Lý do phân công lại không được vượt quá 1000 ký tự.");
    }
}
