using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.RoutingRules.Common;

namespace UrbanIssue.Application.Features.RoutingRules.GetRoutingRuleById;

public sealed class GetRoutingRuleByIdQueryHandler
    : IRequestHandler<
        GetRoutingRuleByIdQuery,
        RoutingRuleResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRoutingRuleByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoutingRuleResult> Handle(
        GetRoutingRuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var routingRule =
            await _dbContext.RoutingRules
                .AsNoTracking()
                .Where(rule =>
                    rule.Id == request.Id)
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
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (routingRule is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy quy tắc định tuyến có ID {request.Id}.");
        }

        return routingRule;
    }
}
