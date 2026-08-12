using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Departments.Common;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Features.Departments.CreateDepartment;

public sealed class CreateDepartmentCommandHandler
    : IRequestHandler<
        CreateDepartmentCommand,
        DepartmentResult>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateDepartmentCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DepartmentResult> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName =
            request.Name.Trim();

        var normalizedDescription =
            string.IsNullOrWhiteSpace(
                request.Description)
                ? null
                : request.Description.Trim();

        var nameExists =
            await _dbContext.Departments
                .AnyAsync(
                    department =>
                        department.Name == normalizedName,
                    cancellationToken);

        if (nameExists)
        {
            throw new ConflictException(
                $"Đơn vị xử lý '{normalizedName}' đã tồn tại.");
        }

        var currentTime =
            DateTime.UtcNow;

        var department =
            new Department
            {
                Name = normalizedName,
                Description = normalizedDescription,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = null
            };

        _dbContext.Departments.Add(
            department);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new DepartmentResult(
            Id: department.Id,
            Name: department.Name,
            Description: department.Description,
            IsActive: department.IsActive,
            CreatedAt: department.CreatedAt,
            UpdatedAt: department.UpdatedAt);
    }
}
