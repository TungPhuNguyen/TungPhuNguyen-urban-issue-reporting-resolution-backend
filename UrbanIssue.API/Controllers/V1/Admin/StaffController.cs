using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Users.Admin;
using UrbanIssue.Application.Features.Users.Admin.Common;
using UrbanIssue.Application.Features.Users.Admin.CreateStaff;
using UrbanIssue.Application.Features.Users.Admin.UpdateStaff;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/staff")]
[Authorize(Roles = "Admin")]
public sealed class StaffController : ControllerBase
{
    private readonly ISender _sender;

    public StaffController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(StaffActionResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffActionResult>>
        Create(
            [FromBody] CreateStaffRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateStaffCommand(
                FullName: request.FullName,
                Email: request.Email,
                Password: request.Password,
                DepartmentId: request.DepartmentId),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(StaffActionResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffActionResult>>
        Update(
            Guid id,
            [FromBody] UpdateStaffRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateStaffCommand(
                StaffId: id,
                FullName: request.FullName,
                Email: request.Email,
                DepartmentId: request.DepartmentId),
            cancellationToken);

        return Ok(result);
    }
}
