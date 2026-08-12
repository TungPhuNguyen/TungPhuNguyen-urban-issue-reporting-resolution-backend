using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Departments.DeleteDepartment;

public sealed class DeleteDepartmentCommandHandler
    : IRequestHandler<
        DeleteDepartmentCommand,
        bool>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteDepartmentCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(
        DeleteDepartmentCommand request,
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

        /*
         * Xóa mềm idempotent.
         */
        if (!department.IsActive)
        {
            return true;
        }

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

        department.IsActive = false;
        department.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
