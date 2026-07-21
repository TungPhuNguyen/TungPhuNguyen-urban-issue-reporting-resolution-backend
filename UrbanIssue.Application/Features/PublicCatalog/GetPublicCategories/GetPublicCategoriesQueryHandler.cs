using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.PublicCatalog.Common;

namespace UrbanIssue.Application.Features.PublicCatalog.GetPublicCategories;

public sealed class GetPublicCategoriesQueryHandler
    : IRequestHandler<
        GetPublicCategoriesQuery,
        IReadOnlyList<PublicCategoryResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPublicCategoriesQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PublicCategoryResult>>
        Handle(
            GetPublicCategoriesQuery request,
            CancellationToken cancellationToken)
    {
        var categories =
            await _dbContext.Categories
                .AsNoTracking()
                .Where(category =>
                    category.IsActive)
                .OrderBy(category =>
                    category.Name)
                .ThenBy(category =>
                    category.Id)
                .Select(category =>
                    new PublicCategoryResult(
                        category.Id,
                        category.Name,
                        category.Description))
                .ToListAsync(
                    cancellationToken);

        return categories;
    }
}
