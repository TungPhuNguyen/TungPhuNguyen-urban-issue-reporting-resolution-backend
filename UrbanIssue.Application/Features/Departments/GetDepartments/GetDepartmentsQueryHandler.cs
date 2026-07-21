using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Departments.Common;

namespace UrbanIssue.Application.Features.Departments.GetDepartments;

public sealed class GetDepartmentsQueryHandler
    : IRequestHandler<
        GetDepartmentsQuery,
        PagedResult<DepartmentResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDepartmentsQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<DepartmentResult>> Handle(
        GetDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var departmentsQuery =
            _dbContext.Departments
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch =
                request.Search.Trim();

            departmentsQuery =
                departmentsQuery.Where(
                    department =>
                        department.Name.Contains(normalizedSearch)
                        || (
                            department.Description != null
                            && department.Description.Contains(
                                normalizedSearch)));
        }

        if (request.IsActive.HasValue)
        {
            departmentsQuery =
                departmentsQuery.Where(
                    department =>
                        department.IsActive
                            == request.IsActive.Value);
        }

        var totalItems =
            await departmentsQuery.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await departmentsQuery
                .OrderBy(department =>
                    department.Name)
                .ThenBy(department =>
                    department.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(department =>
                    new DepartmentResult(
                        department.Id,
                        department.Name,
                        department.Description,
                        department.IsActive,
                        department.CreatedAt,
                        department.UpdatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<DepartmentResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
