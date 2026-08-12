using MediatR;
using UrbanIssue.Application.Features.RoutingRules.Common;

namespace UrbanIssue.Application.Features.RoutingRules.UpdateRoutingRule;

public sealed record UpdateRoutingRuleCommand(
    int Id,
    int CategoryId,
    int AreaId,
    int DepartmentId,
    int PriorityOrder,
    bool IsActive)
    : IRequest<RoutingRuleResult>;
