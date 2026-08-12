using MediatR;
using UrbanIssue.Application.Features.Users.Admin.Common;

namespace UrbanIssue.Application.Features.Users.Admin.UpdateStaff;

public sealed record UpdateStaffCommand(
    Guid StaffId,
    string FullName,
    string Email,
    int DepartmentId)
    : IRequest<StaffActionResult>;
