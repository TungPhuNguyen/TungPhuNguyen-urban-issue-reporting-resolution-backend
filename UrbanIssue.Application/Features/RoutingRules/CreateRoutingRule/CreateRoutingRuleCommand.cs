using MediatR;
using UrbanIssue.Application.Features.RoutingRules.Common;

namespace UrbanIssue.Application.Features.RoutingRules.CreateRoutingRule;

public sealed record CreateRoutingRuleCommand(
    int CategoryId,
    int AreaId,
    int DepartmentId,
    int PriorityOrder)
    : IRequest<RoutingRuleResult>;
