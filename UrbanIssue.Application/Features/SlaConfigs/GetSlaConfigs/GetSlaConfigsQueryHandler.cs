using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.SlaConfigs.Common;

namespace UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigs;

public sealed class GetSlaConfigsQueryHandler
    : IRequestHandler<
        GetSlaConfigsQuery,
        PagedResult<SlaConfigResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSlaConfigsQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SlaConfigResult>> Handle(
        GetSlaConfigsQuery request,
        CancellationToken cancellationToken)
    {
        var slaConfigsQuery =
            _dbContext.SLAConfigs
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch =
                request.Search.Trim();

            slaConfigsQuery =
                slaConfigsQuery.Where(
                    config =>
                        config.Category.Name.Contains(
                            normalizedSearch));
        }

        if (request.CategoryId.HasValue)
        {
            slaConfigsQuery =
                slaConfigsQuery.Where(
                    config =>
                        config.CategoryId
                            == request.CategoryId.Value);
        }

        if (request.Priority.HasValue)
        {
            slaConfigsQuery =
                slaConfigsQuery.Where(
                    config =>
                        config.Priority
                            == request.Priority.Value);
        }

        var totalItems =
            await slaConfigsQuery.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await slaConfigsQuery
                .OrderBy(config =>
                    config.Category.Name)
                .ThenBy(config =>
                    config.Priority)
                .ThenBy(config =>
                    config.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(config =>
                    new SlaConfigResult(
                        config.Id,
                        config.CategoryId,
                        config.Category.Name,
                        config.Priority,
                        config.DurationHours,
                        config.CreatedAt,
                        config.UpdatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<SlaConfigResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
