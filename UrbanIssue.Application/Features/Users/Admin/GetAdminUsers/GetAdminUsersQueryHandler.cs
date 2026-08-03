using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Users.Admin.Common;

namespace UrbanIssue.Application.Features.Users.Admin.GetAdminUsers;

public sealed class GetAdminUsersQueryHandler
    : IRequestHandler<
        GetAdminUsersQuery,
        PagedResult<AdminUserSummaryResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdminUsersQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminUserSummaryResult>>
        Handle(
            GetAdminUsersQuery request,
            CancellationToken cancellationToken)
    {
        var currentTime = DateTime.UtcNow;
        var query = _dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(user =>
                user.FullName.Contains(search)
                || user.Email.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            var roleName = request.RoleName.Trim();

            query = query.Where(user =>
                user.Role.Name == roleName);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(user =>
                user.DepartmentId
                    == request.DepartmentId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(user =>
                user.IsActive == request.IsActive.Value);
        }

        var totalItems = await query.CountAsync(
            cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(
                totalItems / (double)request.PageSize);

        var items = await query
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.FullName)
            .Skip(
                (request.PageNumber - 1)
                * request.PageSize)
            .Take(request.PageSize)
            .Select(user =>
                new AdminUserSummaryResult(
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role.Name,

                    user.DepartmentId,
                    user.Department == null
                        ? null
                        : user.Department.Name,

                    user.IsActive,
                    user.AssignedReports.Count(report =>
                        report.Status == UrbanIssue.Domain.Enums.ReportStatus.Accepted
                        || report.Status == UrbanIssue.Domain.Enums.ReportStatus.InProgress),
                    user.AssignedReports.Count(report =>
                        (report.Status == UrbanIssue.Domain.Enums.ReportStatus.Accepted
                            || report.Status == UrbanIssue.Domain.Enums.ReportStatus.InProgress)
                        && report.DueAt.HasValue
                        && report.DueAt.Value < currentTime),
                    user.CreatedAt,
                    user.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminUserSummaryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
