using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.PostResolution.DismissComplaint;

public sealed class DismissComplaintCommandValidator
    : AbstractValidator<DismissComplaintCommand>
{
    public DismissComplaintCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEmpty()
            .WithMessage(
                "ReportId không được để trống.");

        RuleFor(command => command.Reason)
            .NotEmpty()
            .WithMessage(
                "Lý do không chấp nhận khiếu nại "
                + "không được để trống.")
            .MinimumLength(10)
            .WithMessage(
                "Lý do không chấp nhận khiếu nại "
                + "phải có ít nhất 10 ký tự.")
            .MaximumLength(1000)
            .WithMessage(
                "Lý do không chấp nhận khiếu nại "
                + "không được vượt quá 1000 ký tự.");
    }
}
