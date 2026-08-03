using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Geography;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Areas.UpdateAreaBoundary;

public sealed class UpdateAreaBoundaryCommandHandler
    : IRequestHandler<UpdateAreaBoundaryCommand, AreaBoundaryResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateAreaBoundaryCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaBoundaryResult> Handle(
        UpdateAreaBoundaryCommand request,
        CancellationToken cancellationToken)
    {
        var area = await _dbContext.Areas.SingleOrDefaultAsync(
            item => item.Id == request.AreaId,
            cancellationToken);

        if (area is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy khu vực có ID {request.AreaId}.");
        }

        if (!area.ParentAreaId.HasValue)
        {
            throw new ConflictException(
                "Chỉ cấu hình ranh giới cho phường/xã, không cấu hình trực tiếp cho quận.");
        }

        area.BoundaryGeoJson = string.IsNullOrWhiteSpace(request.GeoJson)
            ? null
            : GeoJsonBoundary.NormalizeGeometry(request.GeoJson);
        area.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AreaBoundaryResult(
            area.Id,
            area.Name,
            area.BoundaryGeoJson is not null,
            area.BoundaryGeoJson,
            area.UpdatedAt);
    }
}
