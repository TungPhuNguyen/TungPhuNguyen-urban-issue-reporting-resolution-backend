using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.AuditLogs.Common;
using UrbanIssue.Application.Features.AuditLogs.GetAuditLogById;
using UrbanIssue.Application.Features.AuditLogs.GetAuditLogs;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public sealed class AuditLogsController
    : ControllerBase
{
    private readonly ISender _sender;

    public AuditLogsController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<AuditLogSummaryResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<PagedResult<AuditLogSummaryResult>>>
        GetAuditLogs(
            [FromQuery] GetAuditLogsQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(AuditLogDetailResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogDetailResult>>
        GetById(
            int id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetAuditLogByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }
}
