using MediatR;
using UrbanIssue.Application.Features.Departments.Common;

namespace UrbanIssue.Application.Features.Departments.UpdateDepartment;

public sealed record UpdateDepartmentCommand(
    int Id,
    string Name,
    string? Description,
    bool IsActive)
    : IRequest<DepartmentResult>;
