using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.RoutingRules.DeleteRoutingRule;

public sealed class DeleteRoutingRuleCommandHandler
    : IRequestHandler<
        DeleteRoutingRuleCommand,
        bool>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteRoutingRuleCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(
        DeleteRoutingRuleCommand request,
        CancellationToken cancellationToken)
    {
        var routingRule =
            await _dbContext.RoutingRules
                .SingleOrDefaultAsync(
                    rule =>
                        rule.Id == request.Id,
                    cancellationToken);

        if (routingRule is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy quy tắc định tuyến có ID {request.Id}.");
        }

        /*
         * Xóa mềm idempotent.
         */
        if (!routingRule.IsActive)
        {
            return true;
        }

        routingRule.IsActive = false;
        routingRule.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
