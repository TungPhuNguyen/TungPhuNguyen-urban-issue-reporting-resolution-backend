using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Users.Admin.Common;

namespace UrbanIssue.Application.Features.Users.Admin.GetAdminUsers;

public sealed record GetAdminUsersQuery(
    string? Search = null,
    string? RoleName = null,
    int? DepartmentId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<AdminUserSummaryResult>>;
