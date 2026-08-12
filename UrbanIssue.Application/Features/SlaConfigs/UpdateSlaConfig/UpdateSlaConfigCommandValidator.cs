using FluentValidation;

namespace UrbanIssue.Application.Features.SlaConfigs.UpdateSlaConfig;

public sealed class UpdateSlaConfigCommandValidator
    : AbstractValidator<UpdateSlaConfigCommand>
{
    public UpdateSlaConfigCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID cấu hình SLA phải lớn hơn 0.");

        RuleFor(command => command.DurationHours)
            .InclusiveBetween(1, 8760)
            .WithMessage(
                "Thời gian SLA phải nằm trong khoảng từ 1 đến 8760 giờ.");
    }
}
