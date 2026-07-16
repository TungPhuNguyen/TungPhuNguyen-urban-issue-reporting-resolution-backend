using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteCategoryCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(
        DeleteCategoryCommand request,
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

        /*
         * Thiết kế theo hướng idempotent:
         * nếu Category đã inactive thì vẫn xem là xóa thành công.
         */
        if (!category.IsActive)
        {
            return true;
        }

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
