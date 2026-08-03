using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Areas.UpdateAreaBoundary;

public sealed class GetAreaBoundaryQueryHandler
    : IRequestHandler<GetAreaBoundaryQuery, AreaBoundaryResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAreaBoundaryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaBoundaryResult> Handle(
        GetAreaBoundaryQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Areas
            .AsNoTracking()
            .Where(area => area.Id == request.AreaId)
            .Select(area => new AreaBoundaryResult(
                area.Id,
                area.Name,
                area.BoundaryGeoJson != null,
                area.BoundaryGeoJson,
                area.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy khu vực có ID {request.AreaId}.");
    }
}
