using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Admin.RejectReport;

public sealed class RejectReportCommandValidator
    : AbstractValidator<RejectReportCommand>
{
    public RejectReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Lý do từ chối không được để trống.")
            .MinimumLength(10)
            .WithMessage("Lý do từ chối phải có ít nhất 10 ký tự.")
            .MaximumLength(1000)
            .WithMessage("Lý do từ chối không được vượt quá 1000 ký tự.");
    }
}
