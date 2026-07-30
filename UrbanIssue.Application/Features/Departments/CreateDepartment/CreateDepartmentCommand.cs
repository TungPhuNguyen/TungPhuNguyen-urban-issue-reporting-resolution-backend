using MediatR;
using UrbanIssue.Application.Features.Departments.Common;

namespace UrbanIssue.Application.Features.Departments.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Name,
    string? Description)
    : IRequest<DepartmentResult>;
