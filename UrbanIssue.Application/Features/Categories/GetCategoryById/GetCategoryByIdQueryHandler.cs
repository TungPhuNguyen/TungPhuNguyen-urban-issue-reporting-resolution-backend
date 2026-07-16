using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Categories.Common;

namespace UrbanIssue.Application.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<
        GetCategoryByIdQuery,
        CategoryResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCategoryByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CategoryResult> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category =
            await _dbContext.Categories
                .AsNoTracking()
                .Where(category =>
                    category.Id == request.Id)
                .Select(category =>
                    new CategoryResult(
                        category.Id,
                        category.Name,
                        category.Description,
                        category.IsActive,
                        category.CreatedAt,
                        category.UpdatedAt))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại sự cố có ID {request.Id}.");
        }

        return category;
    }
}
