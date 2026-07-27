using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Areas.Common;

namespace UrbanIssue.Application.Features.Areas.GetAreas;

public sealed class GetAreasQueryHandler
    : IRequestHandler<
        GetAreasQuery,
        PagedResult<AreaResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAreasQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AreaResult>> Handle(
        GetAreasQuery request,
        CancellationToken cancellationToken)
    {
        var areasQuery =
            _dbContext.Areas
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch =
                request.Search.Trim();

            areasQuery =
    areasQuery.Where(
        area =>
            area.Name.Contains(normalizedSearch)
            || (
                area.Code != null
                && area.Code.Contains(normalizedSearch)
            ));
        }

        if (request.ParentAreaId.HasValue)
        {
            areasQuery =
                areasQuery.Where(
                    area =>
                        area.ParentAreaId
                            == request.ParentAreaId.Value);
        }

        if (request.IsActive.HasValue)
        {
            areasQuery =
                areasQuery.Where(
                    area =>
                        area.IsActive
                            == request.IsActive.Value);
        }

        var totalItems =
            await areasQuery.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await areasQuery
                .OrderBy(area => area.Name)
                .ThenBy(area => area.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(
                    area =>
                        new AreaResult(
                            area.Id,
                            area.Name,
                            area.Code,
                            area.ParentAreaId,
                            area.ParentArea == null
                                ? null
                                : area.ParentArea.Name,
                            area.IsActive,
                            area.CreatedAt,
                            area.UpdatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<AreaResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
