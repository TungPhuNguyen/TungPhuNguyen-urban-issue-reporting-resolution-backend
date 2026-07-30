using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.SlaConfigs;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.SlaConfigs.Common;
using UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigById;
using UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigs;
using UrbanIssue.Application.Features.SlaConfigs.UpdateSlaConfig;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/sla-configs")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public sealed class SlaConfigsController : ControllerBase
{
    private readonly ISender _sender;

    public SlaConfigsController(
        ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy danh sách cấu hình SLA.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<SlaConfigResult>),
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
        ActionResult<PagedResult<SlaConfigResult>>>
        GetSlaConfigs(
            [FromQuery] GetSlaConfigsQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết cấu hình SLA theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(SlaConfigResult),
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
    public async Task<ActionResult<SlaConfigResult>>
        GetSlaConfigById(
            int id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetSlaConfigByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Cập nhật số giờ SLA.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(SlaConfigResult),
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
    public async Task<ActionResult<SlaConfigResult>>
        UpdateSlaConfig(
            int id,
            [FromBody] UpdateSlaConfigRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateSlaConfigCommand(
                Id: id,
                DurationHours: request.DurationHours);

        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(result);
    }
}
