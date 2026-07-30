using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanIssue.API.Contracts.Reports.Admin;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Admin.AssignReport;
using UrbanIssue.Application.Features.Reports.Admin.Common;
using UrbanIssue.Application.Features.Reports.Admin.GetAdminReportById;
using UrbanIssue.Application.Features.Reports.Admin.GetAdminReports;
using UrbanIssue.Application.Features.Reports.Admin.ReassignReport;
using UrbanIssue.Application.Features.Reports.Admin.RejectReport;
using UrbanIssue.Application.Features.Reports.GetReportTimeline;
using UrbanIssue.Application.Features.Reports.PostResolution.CloseReport;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Application.Features.Reports.PostResolution.DismissComplaint;
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

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<AdminReportSummaryResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<PagedResult<AdminReportSummaryResult>>>
        GetReports(
            [FromQuery] GetAdminReportsQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(AdminReportDetailResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminReportDetailResult>>
        GetById(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAdminReportByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }


    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType(
        typeof(GetReportTimelineResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetReportTimelineResult>>
        GetTimeline(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetReportTimelineQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(
        typeof(AdminReportActionResult),
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
    public async Task<ActionResult<AdminReportActionResult>>
        Assign(
            Guid id,
            [FromBody] AssignReportRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignReportCommand(
                ReportId: id,
                DepartmentId: request.DepartmentId,
                StaffId: request.StaffId,
                Note: request.Note),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/reassign")]
    [ProducesResponseType(
        typeof(AdminReportActionResult),
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
    public async Task<ActionResult<AdminReportActionResult>>
        Reassign(
            Guid id,
            [FromBody] ReassignReportRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReassignReportCommand(
                ReportId: id,
                DepartmentId: request.DepartmentId,
                StaffId: request.StaffId,
                Reason: request.Reason),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(
        typeof(AdminReportActionResult),
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
    public async Task<ActionResult<AdminReportActionResult>>
        Reject(
            Guid id,
            [FromBody] RejectReportRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RejectReportCommand(
                ReportId: id,
                Reason: request.Reason),
            cancellationToken);

        return Ok(result);
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
    [HttpPost("{id:guid}/close")]
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
    Close(
        Guid id,
        [FromBody] CloseReportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CloseReportCommand(
                ReportId: id,
                Note: request.Note),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/dismiss-complaint")]
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
        DismissComplaint(
            Guid id,
            [FromBody] DismissComplaintRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new DismissComplaintCommand(
                ReportId: id,
                Reason: request.Reason),
            cancellationToken);

        return Ok(result);
    }
}
