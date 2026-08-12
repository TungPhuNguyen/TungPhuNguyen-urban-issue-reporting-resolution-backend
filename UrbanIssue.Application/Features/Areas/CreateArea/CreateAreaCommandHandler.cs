using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Areas.Common;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Features.Areas.CreateArea;

public sealed class CreateAreaCommandHandler
    : IRequestHandler<CreateAreaCommand, AreaResult>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateAreaCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaResult> Handle(
        CreateAreaCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName =
            request.Name.Trim();

        var normalizedCode =
            request.Code.Trim().ToUpperInvariant();

        string? parentAreaName = null;

        if (request.ParentAreaId.HasValue)
        {
            var parentArea =
                await _dbContext.Areas
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        area =>
                            area.Id == request.ParentAreaId.Value,
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

            parentAreaName = parentArea.Name;
        }

        var codeExists =
            await _dbContext.Areas
                .AnyAsync(
                    area =>
                        area.Code == normalizedCode,
                    cancellationToken);

        if (codeExists)
        {
            throw new ConflictException(
                $"Mã khu vực '{normalizedCode}' đã tồn tại.");
        }

        var nameExistsInSameParent =
            await _dbContext.Areas
                .AnyAsync(
                    area =>
                        area.Name == normalizedName
                        && area.ParentAreaId
                            == request.ParentAreaId,
                    cancellationToken);

        if (nameExistsInSameParent)
        {
            throw new ConflictException(
                $"Khu vực '{normalizedName}' đã tồn tại trong cùng khu vực cha.");
        }

        var currentTime =
            DateTime.UtcNow;

        var area =
            new Area
            {
                Name = normalizedName,
                Code = normalizedCode,
                ParentAreaId = request.ParentAreaId,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = null
            };

        _dbContext.Areas.Add(area);

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
}
