using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.SlaConfigs.Common;

namespace UrbanIssue.Application.Features.SlaConfigs.UpdateSlaConfig;

public sealed class UpdateSlaConfigCommandHandler
    : IRequestHandler<
        UpdateSlaConfigCommand,
        SlaConfigResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSlaConfigCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SlaConfigResult> Handle(
        UpdateSlaConfigCommand request,
        CancellationToken cancellationToken)
    {
        var slaConfig =
            await _dbContext.SLAConfigs
                .Include(config =>
                    config.Category)
                .SingleOrDefaultAsync(
                    config =>
                        config.Id == request.Id,
                    cancellationToken);

        if (slaConfig is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy cấu hình SLA có ID {request.Id}.");
        }

        slaConfig.DurationHours =
            request.DurationHours;

        slaConfig.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new SlaConfigResult(
            Id: slaConfig.Id,
            CategoryId: slaConfig.CategoryId,
            CategoryName: slaConfig.Category.Name,
            Priority: slaConfig.Priority,
            DurationHours: slaConfig.DurationHours,
            CreatedAt: slaConfig.CreatedAt,
            UpdatedAt: slaConfig.UpdatedAt);
    }
}
