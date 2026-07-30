using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Departments;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Departments.Common;
using UrbanIssue.Application.Features.Departments.CreateDepartment;
using UrbanIssue.Application.Features.Departments.DeleteDepartment;
using UrbanIssue.Application.Features.Departments.GetDepartmentById;
using UrbanIssue.Application.Features.Departments.GetDepartments;
using UrbanIssue.Application.Features.Departments.UpdateDepartment;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/departments")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly ISender _sender;

    public DepartmentsController(
        ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy danh sách đơn vị xử lý.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<DepartmentResult>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    public async Task<
        ActionResult<PagedResult<DepartmentResult>>>
        GetDepartments(
            [FromQuery] GetDepartmentsQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết đơn vị xử lý theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(DepartmentResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentResult>>
        GetDepartmentById(
            int id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetDepartmentByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Tạo đơn vị xử lý.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(DepartmentResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentResult>>
        CreateDepartment(
            [FromBody] CreateDepartmentCommand command,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Created(
            $"/api/v1/admin/departments/{result.Id}",
            result);
    }

    /// <summary>
    /// Cập nhật đơn vị xử lý.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(DepartmentResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentResult>>
        UpdateDepartment(
            int id,
            [FromBody] UpdateDepartmentRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateDepartmentCommand(
                Id: id,
                Name: request.Name,
                Description: request.Description,
                IsActive: request.IsActive);

        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Ngừng hoạt động đơn vị xử lý.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult>
        DeleteDepartment(
            int id,
            CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteDepartmentCommand(id),
            cancellationToken);

        return NoContent();
    }
}
