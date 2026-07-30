using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Departments.Common;

namespace UrbanIssue.Application.Features.Departments.GetDepartments;

public sealed record GetDepartmentsQuery(
    string? Search = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<DepartmentResult>>;
