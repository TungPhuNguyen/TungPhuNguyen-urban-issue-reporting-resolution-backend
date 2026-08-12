using FluentValidation;

namespace UrbanIssue.Application.Features.RoutingRules.DeleteRoutingRule;

public sealed class DeleteRoutingRuleCommandValidator
    : AbstractValidator<DeleteRoutingRuleCommand>
{
    public DeleteRoutingRuleCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID quy tắc định tuyến phải lớn hơn 0.");
    }
}
