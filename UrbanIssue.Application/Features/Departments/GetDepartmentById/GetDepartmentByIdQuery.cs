using MediatR;
using UrbanIssue.Application.Features.Departments.Common;

namespace UrbanIssue.Application.Features.Departments.GetDepartmentById;

public sealed record GetDepartmentByIdQuery(
    int Id)
    : IRequest<DepartmentResult>;
