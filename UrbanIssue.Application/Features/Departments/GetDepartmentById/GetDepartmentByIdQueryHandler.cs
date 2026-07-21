using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Departments.Common;

namespace UrbanIssue.Application.Features.Departments.GetDepartmentById;

public sealed class GetDepartmentByIdQueryHandler
    : IRequestHandler<
        GetDepartmentByIdQuery,
        DepartmentResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDepartmentByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DepartmentResult> Handle(
        GetDepartmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var department =
            await _dbContext.Departments
                .AsNoTracking()
                .Where(department =>
                    department.Id == request.Id)
                .Select(department =>
                    new DepartmentResult(
                        department.Id,
                        department.Name,
                        department.Description,
                        department.IsActive,
                        department.CreatedAt,
                        department.UpdatedAt))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy đơn vị xử lý có ID {request.Id}.");
        }

        return department;
    }
}
