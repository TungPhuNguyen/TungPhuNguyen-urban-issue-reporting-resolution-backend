using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.RoutingRules.Common;

namespace UrbanIssue.Application.Features.RoutingRules.GetRoutingRules;

public sealed class GetRoutingRulesQueryHandler
    : IRequestHandler<
        GetRoutingRulesQuery,
        PagedResult<RoutingRuleResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRoutingRulesQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<RoutingRuleResult>> Handle(
        GetRoutingRulesQuery request,
        CancellationToken cancellationToken)
    {
        var routingRulesQuery =
            _dbContext.RoutingRules
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch =
                request.Search.Trim();

            routingRulesQuery =
                routingRulesQuery.Where(
                    rule =>
                        rule.Category.Name.Contains(
                            normalizedSearch)
                        || rule.Area.Name.Contains(
                            normalizedSearch)
                        || rule.Department.Name.Contains(
                            normalizedSearch));
        }

        if (request.CategoryId.HasValue)
        {
            routingRulesQuery =
                routingRulesQuery.Where(
                    rule =>
                        rule.CategoryId
                            == request.CategoryId.Value);
        }

        if (request.AreaId.HasValue)
        {
            routingRulesQuery =
                routingRulesQuery.Where(
                    rule =>
                        rule.AreaId
                            == request.AreaId.Value);
        }

        if (request.DepartmentId.HasValue)
        {
            routingRulesQuery =
                routingRulesQuery.Where(
                    rule =>
                        rule.DepartmentId
                            == request.DepartmentId.Value);
        }

        if (request.IsActive.HasValue)
        {
            routingRulesQuery =
                routingRulesQuery.Where(
                    rule =>
                        rule.IsActive
                            == request.IsActive.Value);
        }

        var totalItems =
            await routingRulesQuery.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await routingRulesQuery
                .OrderBy(rule =>
                    rule.PriorityOrder)
                .ThenBy(rule =>
                    rule.Category.Name)
                .ThenBy(rule =>
                    rule.Area.Name)
                .ThenBy(rule =>
                    rule.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(rule =>
                    new RoutingRuleResult(
                        rule.Id,
                        rule.CategoryId,
                        rule.Category.Name,
                        rule.AreaId,
                        rule.Area.Name,
                        rule.DepartmentId,
                        rule.Department.Name,
                        rule.PriorityOrder,
                        rule.IsActive,
                        rule.CreatedAt,
                        rule.UpdatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<RoutingRuleResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
