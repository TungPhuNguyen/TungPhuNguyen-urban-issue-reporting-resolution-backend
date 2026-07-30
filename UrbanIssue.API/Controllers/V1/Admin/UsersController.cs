using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Users.Admin;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Users.Admin.ChangeUserStatus;
using UrbanIssue.Application.Features.Users.Admin.Common;
using UrbanIssue.Application.Features.Users.Admin.GetAdminUserById;
using UrbanIssue.Application.Features.Users.Admin.GetAdminUsers;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<AdminUserSummaryResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<PagedResult<AdminUserSummaryResult>>>
        GetUsers(
            [FromQuery] GetAdminUsersQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(AdminUserDetailResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDetailResult>>
        GetById(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAdminUserByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(
        typeof(UserStatusActionResult),
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
    public async Task<ActionResult<UserStatusActionResult>>
        ChangeStatus(
            Guid id,
            [FromBody] ChangeUserStatusRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ChangeUserStatusCommand(
                UserId: id,
                IsActive: request.IsActive,
                Reason: request.Reason),
            cancellationToken);

        return Ok(result);
    }
}
