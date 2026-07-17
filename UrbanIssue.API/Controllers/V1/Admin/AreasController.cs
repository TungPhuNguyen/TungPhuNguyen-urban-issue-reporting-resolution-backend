using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Areas;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Areas.Common;
using UrbanIssue.Application.Features.Areas.CreateArea;
using UrbanIssue.Application.Features.Areas.DeleteArea;
using UrbanIssue.Application.Features.Areas.GetAreaById;
using UrbanIssue.Application.Features.Areas.GetAreas;
using UrbanIssue.Application.Features.Areas.UpdateArea;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/areas")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public sealed class AreasController : ControllerBase
{
    private readonly ISender _sender;

    public AreasController(
        ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy danh sách khu vực.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<AreaResult>),
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
        ActionResult<PagedResult<AreaResult>>>
        GetAreas(
            [FromQuery] GetAreasQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết khu vực theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(AreaResult),
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
    public async Task<ActionResult<AreaResult>>
        GetAreaById(
            int id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetAreaByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Tạo khu vực mới.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(AreaResult),
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
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AreaResult>>
        CreateArea(
            [FromBody] CreateAreaCommand command,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Created(
            $"/api/v1/admin/areas/{result.Id}",
            result);
    }

    /// <summary>
    /// Cập nhật khu vực.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(AreaResult),
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
    public async Task<ActionResult<AreaResult>>
        UpdateArea(
            int id,
            [FromBody] UpdateAreaRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateAreaCommand(
                Id: id,
                Name: request.Name,
                Code: request.Code,
                ParentAreaId: request.ParentAreaId,
                IsActive: request.IsActive);

        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Ngừng hoạt động khu vực.
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
        DeleteArea(
            int id,
            CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteAreaCommand(id),
            cancellationToken);

        return NoContent();
    }
}
