using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;

public sealed class SubmitComplaintCommandValidator
    : AbstractValidator<SubmitComplaintCommand>
{
    public SubmitComplaintCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Lý do khiếu nại không được để trống.")
            .MinimumLength(10)
            .WithMessage("Lý do khiếu nại phải có ít nhất 10 ký tự.")
            .MaximumLength(2000)
            .WithMessage("Lý do khiếu nại không được vượt quá 2000 ký tự.");
    }
}
