using FluentValidation;

namespace UrbanIssue.Application.Features.RoutingRules.GetRoutingRuleById;

public sealed class GetRoutingRuleByIdQueryValidator
    : AbstractValidator<GetRoutingRuleByIdQuery>
{
    public GetRoutingRuleByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID quy tắc định tuyến phải lớn hơn 0.");
    }
}
