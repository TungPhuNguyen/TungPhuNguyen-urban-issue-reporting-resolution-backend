using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Departments.Common;

namespace UrbanIssue.Application.Features.Departments.UpdateDepartment;

public sealed class UpdateDepartmentCommandHandler
    : IRequestHandler<
        UpdateDepartmentCommand,
        DepartmentResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateDepartmentCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DepartmentResult> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department =
            await _dbContext.Departments
                .SingleOrDefaultAsync(
                    department =>
                        department.Id == request.Id,
                    cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy đơn vị xử lý có ID {request.Id}.");
        }

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
                    otherDepartment =>
                        otherDepartment.Id != request.Id
                        && otherDepartment.Name
                            == normalizedName,
                    cancellationToken);

        if (nameExists)
        {
            throw new ConflictException(
                $"Đơn vị xử lý '{normalizedName}' đã tồn tại.");
        }

        if (department.IsActive
            && !request.IsActive)
        {
            var hasActiveRoutingRules =
                await _dbContext.RoutingRules
                    .AnyAsync(
                        rule =>
                            rule.DepartmentId == request.Id
                            && rule.IsActive,
                        cancellationToken);

            if (hasActiveRoutingRules)
            {
                throw new ConflictException(
                    "Không thể ngừng hoạt động đơn vị đang được sử dụng trong quy tắc định tuyến.");
            }

            var hasActiveUsers =
                await _dbContext.Users
                    .AnyAsync(
                        user =>
                            user.DepartmentId == request.Id
                            && user.IsActive,
                        cancellationToken);

            if (hasActiveUsers)
            {
                throw new ConflictException(
                    "Không thể ngừng hoạt động đơn vị đang có tài khoản nhân viên hoạt động.");
            }
        }

        department.Name = normalizedName;
        department.Description = normalizedDescription;
        department.IsActive = request.IsActive;
        department.UpdatedAt = DateTime.UtcNow;

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
