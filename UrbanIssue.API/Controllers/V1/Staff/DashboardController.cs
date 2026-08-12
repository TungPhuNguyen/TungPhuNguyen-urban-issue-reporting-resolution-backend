using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Features.Reports.Staff.Dashboard.Common;
using UrbanIssue.Application.Features.Reports.Staff.Dashboard.GetStaffDashboard;

namespace UrbanIssue.API.Controllers.V1.Staff;

[ApiController]
[Route("api/v1/staff/dashboard")]
[Authorize(Roles = "Staff")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("summary")]
    [ProducesResponseType(
        typeof(StaffDashboardResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StaffDashboardResult>>
        GetSummary(
            [FromQuery] GetStaffDashboardQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }
}
