using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.RoutingRules.Common;

namespace UrbanIssue.Application.Features.RoutingRules.GetRoutingRules;

public sealed record GetRoutingRulesQuery(
    string? Search = null,
    int? CategoryId = null,
    int? AreaId = null,
    int? DepartmentId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<RoutingRuleResult>>;
