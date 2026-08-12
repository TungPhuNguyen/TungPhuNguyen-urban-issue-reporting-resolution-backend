namespace UrbanIssue.API.Contracts.RoutingRules;

public sealed record UpdateRoutingRuleRequest(
    int CategoryId,
    int AreaId,
    int DepartmentId,
    int PriorityOrder,
    bool IsActive);
