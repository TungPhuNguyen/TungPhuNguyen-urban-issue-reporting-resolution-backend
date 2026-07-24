using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Reports.Admin;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Application.Features.Reports.PostResolution.ReopenReport;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/reports")]
[Authorize(Roles = "Admin")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(
        typeof(PostResolutionActionResult),
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
    public async Task<ActionResult<PostResolutionActionResult>>
        Reopen(
            Guid id,
            [FromBody] ReopenReportRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReopenReportCommand(
                ReportId: id,
                Reason: request.Reason),
            cancellationToken);

        return Ok(result);
    }
}
