using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Users.Admin.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Users.Admin.GetAdminUserById;

public sealed class GetAdminUserByIdQueryHandler
    : IRequestHandler<
        GetAdminUserByIdQuery,
        AdminUserDetailResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdminUserByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminUserDetailResult> Handle(
        GetAdminUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == request.UserId)
            .Select(user =>
                new AdminUserDetailResult(
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role.Name,

                    user.DepartmentId,
                    user.Department == null
                        ? null
                        : user.Department.Name,

                    user.IsActive,

                    user.CreatedReports.Count(),

                    user.AssignedReports.Count(),

                    user.AssignedReports.Count(report =>
                        report.Status == ReportStatus.Assigned
                        || report.Status == ReportStatus.Accepted
                        || report.Status == ReportStatus.InProgress),

                    user.CreatedAt,
                    user.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy người dùng có ID {request.UserId}.");
        }

        return result;
    }
}
