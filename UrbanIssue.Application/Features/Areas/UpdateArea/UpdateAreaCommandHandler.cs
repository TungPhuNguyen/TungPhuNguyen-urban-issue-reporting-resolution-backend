using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Areas.Common;

namespace UrbanIssue.Application.Features.Areas.UpdateArea;

public sealed class UpdateAreaCommandHandler
    : IRequestHandler<UpdateAreaCommand, AreaResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateAreaCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaResult> Handle(
        UpdateAreaCommand request,
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

        var normalizedName =
            request.Name.Trim();

        var normalizedCode =
            request.Code.Trim().ToUpperInvariant();

        string? parentAreaName = null;

        if (request.ParentAreaId.HasValue)
        {
            if (request.ParentAreaId.Value == request.Id)
            {
                throw new ConflictException(
                    "Khu vực không thể là khu vực cha của chính nó.");
            }

            var parentArea =
                await _dbContext.Areas
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        parent =>
                            parent.Id
                                == request.ParentAreaId.Value,
                        cancellationToken);

            if (parentArea is null)
            {
                throw new KeyNotFoundException(
                    $"Không tìm thấy khu vực cha có ID {request.ParentAreaId.Value}.");
            }

            if (!parentArea.IsActive)
            {
                throw new ConflictException(
                    "Không thể chọn một khu vực cha đã ngừng hoạt động.");
            }

            await EnsureHierarchyDoesNotContainCycle(
                request.Id,
                request.ParentAreaId,
                cancellationToken);

            parentAreaName = parentArea.Name;
        }

        var codeExists =
            await _dbContext.Areas
                .AnyAsync(
                    otherArea =>
                        otherArea.Id != request.Id
                        && otherArea.Code
                            == normalizedCode,
                    cancellationToken);

        if (codeExists)
        {
            throw new ConflictException(
                $"Mã khu vực '{normalizedCode}' đã tồn tại.");
        }

        var nameExistsInSameParent =
            await _dbContext.Areas
                .AnyAsync(
                    otherArea =>
                        otherArea.Id != request.Id
                        && otherArea.Name
                            == normalizedName
                        && otherArea.ParentAreaId
                            == request.ParentAreaId,
                    cancellationToken);

        if (nameExistsInSameParent)
        {
            throw new ConflictException(
                $"Khu vực '{normalizedName}' đã tồn tại trong cùng khu vực cha.");
        }

        if (area.IsActive && !request.IsActive)
        {
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
        }

        area.Name = normalizedName;
        area.Code = normalizedCode;
        area.ParentAreaId = request.ParentAreaId;
        area.IsActive = request.IsActive;
        area.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AreaResult(
            Id: area.Id,
            Name: area.Name,
            Code: area.Code,
            ParentAreaId: area.ParentAreaId,
            ParentAreaName: parentAreaName,
            IsActive: area.IsActive,
            CreatedAt: area.CreatedAt,
            UpdatedAt: area.UpdatedAt);
    }

    private async Task EnsureHierarchyDoesNotContainCycle(
        int areaId,
        int? proposedParentAreaId,
        CancellationToken cancellationToken)
    {
        var visitedAreaIds =
            new HashSet<int>();

        var currentAreaId =
            proposedParentAreaId;

        while (currentAreaId.HasValue)
        {
            if (currentAreaId.Value == areaId)
            {
                throw new ConflictException(
                    "Không thể cập nhật vì thao tác này tạo vòng lặp trong cây khu vực.");
            }

            if (!visitedAreaIds.Add(
                    currentAreaId.Value))
            {
                throw new ConflictException(
                    "Cấu trúc cây khu vực hiện tại có vòng lặp không hợp lệ.");
            }

            currentAreaId =
                await _dbContext.Areas
                    .AsNoTracking()
                    .Where(area =>
                        area.Id == currentAreaId.Value)
                    .Select(area =>
                        area.ParentAreaId)
                    .SingleOrDefaultAsync(
                        cancellationToken);
        }
    }
}
