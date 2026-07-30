using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.AcceptReport;

public sealed class AcceptReportCommandValidator
    : AbstractValidator<AcceptReportCommand>
{
    public AcceptReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Mức độ ưu tiên không hợp lệ.");

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage("Ghi chú không được vượt quá 1000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
