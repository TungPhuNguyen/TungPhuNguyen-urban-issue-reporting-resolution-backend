using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Admin.ClassifyReport;

public sealed class ClassifyReportCommandValidator
    : AbstractValidator<ClassifyReportCommand>
{
    public ClassifyReportCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(command => command.CategoryId)
            .GreaterThan(0)
            .WithMessage("ID loại sự cố phải lớn hơn 0.");

        RuleFor(command => command.Note)
            .MaximumLength(1000)
            .WithMessage("Ghi chú phân loại không được vượt quá 1000 ký tự.")
            .When(command => !string.IsNullOrWhiteSpace(command.Note));
    }
}
