using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Features.Dashboard.Common;
using UrbanIssue.Application.Features.Dashboard.GetDashboardSummary;
using UrbanIssue.Application.Features.Dashboard.GetReportTrend;
using UrbanIssue.Application.Features.Dashboard.GetReportsByArea;
using UrbanIssue.Application.Features.Dashboard.GetReportsByCategory;
using UrbanIssue.Application.Features.Dashboard.GetReportsByStatus;
using UrbanIssue.Application.Features.Dashboard.GetSlaPerformance;

namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Roles = "Admin")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("summary")]
    [ProducesResponseType(
        typeof(DashboardSummaryResult),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryResult>>
        GetSummary(
            [FromQuery] GetDashboardSummaryQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("reports-by-status")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReportsByStatusResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyList<ReportsByStatusResult>>>
        GetReportsByStatus(
            [FromQuery] GetReportsByStatusQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("reports-by-category")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReportsByCategoryResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyList<ReportsByCategoryResult>>>
        GetReportsByCategory(
            [FromQuery] GetReportsByCategoryQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("reports-by-area")]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReportsByAreaResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyList<ReportsByAreaResult>>>
        GetReportsByArea(
            [FromQuery] GetReportsByAreaQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("sla-performance")]
    [ProducesResponseType(
        typeof(SlaPerformanceResult),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<SlaPerformanceResult>>
        GetSlaPerformance(
            [FromQuery] GetSlaPerformanceQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("report-trend")]
    [ProducesResponseType(
        typeof(ReportTrendResult),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportTrendResult>>
        GetReportTrend(
            [FromQuery] GetReportTrendQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }
}
