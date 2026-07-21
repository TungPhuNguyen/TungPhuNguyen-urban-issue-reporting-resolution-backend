using MediatR;

namespace UrbanIssue.Application.Features.RoutingRules.DeleteRoutingRule;

public sealed record DeleteRoutingRuleCommand(
    int Id)
    : IRequest<bool>;
