using MediatR;
using UrbanIssue.Application.Features.Users.Admin.Common;

namespace UrbanIssue.Application.Features.Users.Admin.CreateStaff;

public sealed record CreateStaffCommand(
    string FullName,
    string Email,
    string Password,
    int DepartmentId)
    : IRequest<StaffActionResult>;
