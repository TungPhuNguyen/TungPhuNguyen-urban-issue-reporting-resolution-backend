using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Areas.Common;

namespace UrbanIssue.Application.Features.Areas.GetAreaById;

public sealed class GetAreaByIdQueryHandler
    : IRequestHandler<GetAreaByIdQuery, AreaResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAreaByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaResult> Handle(
        GetAreaByIdQuery request,
        CancellationToken cancellationToken)
    {
        var area =
            await _dbContext.Areas
                .AsNoTracking()
                .Where(area =>
                    area.Id == request.Id)
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
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (area is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy khu vực có ID {request.Id}.");
        }

        return area;
    }
}
