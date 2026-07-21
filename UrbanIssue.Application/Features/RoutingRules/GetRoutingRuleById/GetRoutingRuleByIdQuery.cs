using MediatR;
using UrbanIssue.Application.Features.RoutingRules.Common;

namespace UrbanIssue.Application.Features.RoutingRules.GetRoutingRuleById;

public sealed record GetRoutingRuleByIdQuery(
    int Id)
    : IRequest<RoutingRuleResult>;
