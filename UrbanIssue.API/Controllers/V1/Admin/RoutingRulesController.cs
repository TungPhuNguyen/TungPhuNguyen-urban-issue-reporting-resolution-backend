using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.RoutingRules;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.RoutingRules.Common;
using UrbanIssue.Application.Features.RoutingRules.CreateRoutingRule;
using UrbanIssue.Application.Features.RoutingRules.DeleteRoutingRule;
using UrbanIssue.Application.Features.RoutingRules.GetRoutingRuleById;
using UrbanIssue.Application.Features.RoutingRules.GetRoutingRules;
using UrbanIssue.Application.Features.RoutingRules.UpdateRoutingRule;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/routing-rules")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public sealed class RoutingRulesController : ControllerBase
{
    private readonly ISender _sender;

    public RoutingRulesController(
        ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy danh sách quy tắc định tuyến.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<RoutingRuleResult>),
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
        ActionResult<PagedResult<RoutingRuleResult>>>
        GetRoutingRules(
            [FromQuery] GetRoutingRulesQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết quy tắc định tuyến.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(RoutingRuleResult),
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
    public async Task<ActionResult<RoutingRuleResult>>
        GetRoutingRuleById(
            int id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetRoutingRuleByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Tạo quy tắc định tuyến.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(RoutingRuleResult),
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
    public async Task<ActionResult<RoutingRuleResult>>
        CreateRoutingRule(
            [FromBody] CreateRoutingRuleCommand command,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Created(
            $"/api/v1/admin/routing-rules/{result.Id}",
            result);
    }

    /// <summary>
    /// Cập nhật quy tắc định tuyến.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(RoutingRuleResult),
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
    public async Task<ActionResult<RoutingRuleResult>>
        UpdateRoutingRule(
            int id,
            [FromBody] UpdateRoutingRuleRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateRoutingRuleCommand(
                Id: id,
                CategoryId: request.CategoryId,
                AreaId: request.AreaId,
                DepartmentId: request.DepartmentId,
                PriorityOrder: request.PriorityOrder,
                IsActive: request.IsActive);

        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Ngừng hoạt động quy tắc định tuyến.
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
    public async Task<IActionResult>
        DeleteRoutingRule(
            int id,
            CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteRoutingRuleCommand(id),
            cancellationToken);

        return NoContent();
    }
}
