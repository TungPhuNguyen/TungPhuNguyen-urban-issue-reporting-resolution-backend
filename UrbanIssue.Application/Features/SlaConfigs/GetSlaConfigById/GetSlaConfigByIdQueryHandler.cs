using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.SlaConfigs.Common;

namespace UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigById;

public sealed class GetSlaConfigByIdQueryHandler
    : IRequestHandler<
        GetSlaConfigByIdQuery,
        SlaConfigResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSlaConfigByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SlaConfigResult> Handle(
        GetSlaConfigByIdQuery request,
        CancellationToken cancellationToken)
    {
        var slaConfig =
            await _dbContext.SLAConfigs
                .AsNoTracking()
                .Where(config =>
                    config.Id == request.Id)
                .Select(config =>
                    new SlaConfigResult(
                        config.Id,
                        config.CategoryId,
                        config.Category.Name,
                        config.Priority,
                        config.DurationHours,
                        config.CreatedAt,
                        config.UpdatedAt))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (slaConfig is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy cấu hình SLA có ID {request.Id}.");
        }

        return slaConfig;
    }
}
