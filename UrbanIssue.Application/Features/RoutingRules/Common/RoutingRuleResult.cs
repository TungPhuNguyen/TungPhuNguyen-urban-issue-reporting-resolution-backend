namespace UrbanIssue.Application.Features.RoutingRules.Common;

public sealed record RoutingRuleResult(
    int Id,
    int CategoryId,
    string CategoryName,
    int AreaId,
    string AreaName,
    int DepartmentId,
    string DepartmentName,
    int PriorityOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
