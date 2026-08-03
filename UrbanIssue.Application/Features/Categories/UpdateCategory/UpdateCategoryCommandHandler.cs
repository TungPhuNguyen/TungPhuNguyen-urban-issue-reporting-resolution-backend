using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Categories.Common;

namespace UrbanIssue.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<
        UpdateCategoryCommand,
        CategoryResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateCategoryCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CategoryResult> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category =
            await _dbContext.Categories
                .SingleOrDefaultAsync(
                    category =>
                        category.Id == request.Id,
                    cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại sự cố có ID {request.Id}.");
        }

        var normalizedName =
            request.Name.Trim();

        var normalizedDescription =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        /*
         * Kiểm tra tên có thuộc về Category khác không.
         *
         * Category hiện tại được loại ra bằng:
         * category.Id != request.Id
         */
        var categoryNameExists =
            await _dbContext.Categories
                .AnyAsync(
                    otherCategory =>
                        otherCategory.Id != request.Id
                        && otherCategory.Name
                            == normalizedName,
                    cancellationToken);

        if (categoryNameExists)
        {
            throw new ConflictException(
                $"Loại sự cố '{normalizedName}' đã tồn tại.");
        }

        category.Name =
            normalizedName;

        category.Description =
            normalizedDescription;

        category.IsActive =
            request.IsActive;

        category.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new CategoryResult(
            Id: category.Id,
            Name: category.Name,
            Description: category.Description,
            IsOther: category.IsOther,
            IsActive: category.IsActive,
            CreatedAt: category.CreatedAt,
            UpdatedAt: category.UpdatedAt);
    }
}
