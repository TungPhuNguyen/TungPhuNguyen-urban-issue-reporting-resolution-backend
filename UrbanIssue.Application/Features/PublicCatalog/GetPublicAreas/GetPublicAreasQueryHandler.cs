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

    public async Task<IReadOnlyList<PublicAreaResult>> Handle(
        GetPublicAreasQuery request,
        CancellationToken cancellationToken)
    {
        var areasQuery = _dbContext.Areas
            .AsNoTracking()
            .Where(area => area.IsActive);

        /*
         * Khi có ParentAreaId, kiểm tra ID đó phải là
         * một Quận đang hoạt động.
         */
        if (request.ParentAreaId.HasValue)
        {
            var districtId =
                request.ParentAreaId.Value;

            var districtExists =
                await _dbContext.Areas
                    .AsNoTracking()
                    .AnyAsync(
                        area =>
                            area.Id == districtId
                            && area.IsActive
                            && !area.ParentAreaId.HasValue,
                        cancellationToken);

            if (!districtExists)
            {
                throw new KeyNotFoundException(
                    $"Không tìm thấy Quận đang hoạt động "
                    + $"có ID {districtId}.");
            }

            /*
             * Lấy các Phường trực thuộc Quận.
             */
            areasQuery = areasQuery.Where(
                area =>
                    area.ParentAreaId == districtId);
        }
        else
        {
            /*
             * Không truyền ParentAreaId thì chỉ lấy Quận.
             */
            areasQuery = areasQuery.Where(
                area =>
                    !area.ParentAreaId.HasValue);
        }

        return await areasQuery
            .OrderBy(area => area.Name)
            .Select(area =>
                new PublicAreaResult(
                    area.Id,
                    area.Name,
                    area.Code,
                    area.ParentAreaId))
            .ToListAsync(cancellationToken);
    }
}
