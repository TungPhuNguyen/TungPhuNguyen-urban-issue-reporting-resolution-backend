using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Categories.Common;

namespace UrbanIssue.Application.Features.Categories.GetCategories;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<
        GetCategoriesQuery,
        PagedResult<CategoryResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCategoriesQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CategoryResult>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categoriesQuery =
            _dbContext.Categories
                .AsNoTracking()
                .AsQueryable();

        /*
         * Tìm kiếm theo tên Category.
         */
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch =
                request.Search.Trim();

            categoriesQuery =
                categoriesQuery.Where(
                    category =>
                        category.Name.Contains(normalizedSearch));
        }

        /*
         * Lọc theo trạng thái hoạt động.
         *
         * null  -> lấy tất cả
         * true  -> chỉ đang hoạt động
         * false -> chỉ đã ngừng hoạt động
         */
        if (request.IsActive.HasValue)
        {
            categoriesQuery =
                categoriesQuery.Where(
                    category =>
                        category.IsActive
                            == request.IsActive.Value);
        }

        /*
         * Đếm tổng số bản ghi sau khi áp dụng bộ lọc,
         * nhưng trước khi phân trang.
         */
        var totalItems =
            await categoriesQuery.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        /*
         * Luôn sắp xếp trước khi Skip và Take
         * để kết quả phân trang ổn định.
         */
        var items =
            await categoriesQuery
                .OrderBy(category => category.IsOther)
                .ThenBy(category => category.Name)
                .ThenBy(category => category.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(
                    category =>
                        new CategoryResult(
                            category.Id,
                            category.Name,
                            category.Description,
                            category.IsOther,
                            category.IsActive,
                            category.CreatedAt,
                            category.UpdatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<CategoryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
