using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.CancelReport;

public sealed class CancelReportCommandValidator
    : AbstractValidator<CancelReportCommand>
{
    public CancelReportCommandValidator()
    {
        RuleFor(command => command.ReportId).NotEqual(Guid.Empty);
        RuleFor(command => command.Reason)
            .NotEmpty().MinimumLength(5).MaximumLength(1000);
        RuleFor(command => command.RowVersion)
            .NotEmpty()
            .WithMessage("RowVersion là bắt buộc để tránh ghi đè dữ liệu mới hơn.");
    }
}
