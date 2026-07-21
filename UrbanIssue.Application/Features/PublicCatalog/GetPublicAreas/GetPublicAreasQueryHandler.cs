using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.PublicCatalog.Common;

namespace UrbanIssue.Application.Features.PublicCatalog.GetPublicAreas;

public sealed class GetPublicAreasQueryHandler
    : IRequestHandler<
        GetPublicAreasQuery,
        IReadOnlyList<PublicAreaResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPublicAreasQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PublicAreaResult>>
        Handle(
            GetPublicAreasQuery request,
            CancellationToken cancellationToken)
    {
        var areasQuery =
            _dbContext.Areas
                .AsNoTracking()
                .Where(area =>
                    area.IsActive);

        if (request.ParentAreaId.HasValue)
        {
            /*
             * Lấy các khu vực con trực tiếp
             * của ParentAreaId được truyền vào.
             */
            areasQuery =
                areasQuery.Where(area =>
                    area.ParentAreaId
                        == request.ParentAreaId.Value);
        }
        else
        {
            /*
             * Không truyền ParentAreaId:
             * chỉ lấy các khu vực cấp gốc.
             */
            areasQuery =
                areasQuery.Where(area =>
                    area.ParentAreaId == null);
        }

        var areas =
            await areasQuery
                .OrderBy(area =>
                    area.Name)
                .ThenBy(area =>
                    area.Id)
                .Select(area =>
                    new PublicAreaResult(
                        area.Id,
                        area.Name,
                        area.Code,
                        area.ParentAreaId))
                .ToListAsync(
                    cancellationToken);

        return areas;
    }
}
