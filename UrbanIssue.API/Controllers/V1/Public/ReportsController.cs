using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Public.Common;
using UrbanIssue.Application.Features.Reports.Public.GetPublicReportById;
using UrbanIssue.Application.Features.Reports.Public.GetPublicReports;

namespace UrbanIssue.API.Controllers.V1.Public;

[ApiController]
[Route("api/v1/public/reports")]
[AllowAnonymous]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(
        ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy danh sách báo cáo công khai cho bản đồ.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<PublicReportMapItemResult>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<PagedResult<PublicReportMapItemResult>>>
        GetReports(
            [FromQuery] GetPublicReportsQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy chi tiết một báo cáo công khai.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(PublicReportDetailResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicReportDetailResult>>
        GetById(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetPublicReportByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }
}
