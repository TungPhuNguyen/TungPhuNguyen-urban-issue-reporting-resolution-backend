using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Areas.DeleteArea;

public sealed class DeleteAreaCommandHandler
    : IRequestHandler<DeleteAreaCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteAreaCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(
        DeleteAreaCommand request,
        CancellationToken cancellationToken)
    {
        var area =
            await _dbContext.Areas
                .SingleOrDefaultAsync(
                    area =>
                        area.Id == request.Id,
                    cancellationToken);

        if (area is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy khu vực có ID {request.Id}.");
        }

        if (!area.IsActive)
        {
            return true;
        }

        var hasActiveChildren =
            await _dbContext.Areas
                .AnyAsync(
                    child =>
                        child.ParentAreaId == request.Id
                        && child.IsActive,
                    cancellationToken);

        if (hasActiveChildren)
        {
            throw new ConflictException(
                "Không thể ngừng hoạt động khu vực đang có khu vực con hoạt động.");
        }

        area.IsActive = false;
        area.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
