using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Geography;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Common.Geography;

public sealed class AreaBoundaryService
    : IAreaBoundaryService
{
    private readonly IApplicationDbContext _dbContext;

    public AreaBoundaryService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaBoundaryCheckResult> CheckAsync(
        int areaId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken)
    {
        var boundary = await _dbContext.Areas
            .AsNoTracking()
            .Where(area => area.Id == areaId)
            .Select(area => area.BoundaryGeoJson)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(boundary))
        {
            return new AreaBoundaryCheckResult(false, false);
        }

        return new AreaBoundaryCheckResult(
            true,
            GeoJsonBoundary.Contains(boundary, latitude, longitude));
    }

    public async Task<AreaLocationMatch?> FindContainingWardAsync(
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken)
    {
        var candidates = await _dbContext.Areas
            .AsNoTracking()
            .Where(area => area.IsActive
                && area.ParentAreaId.HasValue
                && area.BoundaryGeoJson != null)
            .Select(area => new
            {
                area.Id,
                area.Name,
                area.Code,
                area.ParentAreaId,
                DistrictName = area.ParentArea!.Name,
                area.BoundaryGeoJson
            })
            .ToListAsync(cancellationToken);

        foreach (var area in candidates)
        {
            if (GeoJsonBoundary.Contains(
                    area.BoundaryGeoJson!,
                    latitude,
                    longitude))
            {
                return new AreaLocationMatch(
                    area.Id,
                    area.Name,
                    area.Code,
                    area.ParentAreaId!.Value,
                    area.DistrictName);
            }
        }

        return null;
    }
}
