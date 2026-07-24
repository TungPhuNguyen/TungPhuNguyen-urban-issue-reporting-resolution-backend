using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.PostResolution.CloseReport;

public sealed class CloseReportCommandValidator
    : AbstractValidator<CloseReportCommand>
{
    public CloseReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.Note)
            .MaximumLength(1000)
            .WithMessage(
                "Ghi chú không được vượt quá 1000 ký tự.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.Note));
    }
}
