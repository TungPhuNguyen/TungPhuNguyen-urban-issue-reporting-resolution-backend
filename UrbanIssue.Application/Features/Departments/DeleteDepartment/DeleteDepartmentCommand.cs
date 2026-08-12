using MediatR;

namespace UrbanIssue.Application.Features.Departments.DeleteDepartment;

public sealed record DeleteDepartmentCommand(
    int Id)
    : IRequest<bool>;
