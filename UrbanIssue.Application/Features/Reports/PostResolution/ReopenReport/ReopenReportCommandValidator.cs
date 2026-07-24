using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.PostResolution.ReopenReport;

public sealed class ReopenReportCommandValidator
    : AbstractValidator<ReopenReportCommand>
{
    public ReopenReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Lý do mở lại báo cáo không được để trống.")
            .MinimumLength(10)
            .WithMessage(
                "Lý do mở lại báo cáo phải có ít nhất 10 ký tự.")
            .MaximumLength(2000)
            .WithMessage(
                "Lý do mở lại báo cáo không được vượt quá 2000 ký tự.");
    }
}
