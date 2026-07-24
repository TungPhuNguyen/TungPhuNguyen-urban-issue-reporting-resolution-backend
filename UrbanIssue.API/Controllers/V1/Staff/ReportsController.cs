using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Reports.Staff;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Staff.AcceptReport;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Application.Features.Reports.Staff.GetDepartmentReports;
using UrbanIssue.Application.Features.Reports.Staff.GetStaffReportById;
using UrbanIssue.Application.Features.Reports.Staff.ResolveReport;
using UrbanIssue.Application.Features.Reports.Staff.StartProcessingReport;

namespace UrbanIssue.API.Controllers.V1.Staff;

[ApiController]
[Route("api/v1/staff/reports")]
[Authorize(Roles = "Staff")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<
        ActionResult<PagedResult<StaffReportSummaryResult>>>
        GetReports(
            [FromQuery] GetDepartmentReportsQuery query,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StaffReportDetailResult>>
        GetById(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetStaffReportByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<StaffReportActionResult>>
        Accept(
            Guid id,
            [FromBody] AcceptReportRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AcceptReportCommand(
                ReportId: id,
                Priority: request.Priority,
                Note: request.Note),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/start-processing")]
    public async Task<ActionResult<StaffReportActionResult>>
        StartProcessing(
            Guid id,
            [FromBody] StartProcessingReportRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new StartProcessingReportCommand(
                ReportId: id,
                Note: request.Note),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/resolve")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StaffReportActionResult>>
        Resolve(
            Guid id,
            [FromForm] ResolveReportRequest request,
            CancellationToken cancellationToken)
    {
        var uploadFiles = request.Images
            .Select(file =>
                new UploadFile(
                    FileName: file.FileName,
                    ContentType: file.ContentType,
                    Length: file.Length,
                    Content: file.OpenReadStream()))
            .ToList();

        try
        {
            var result = await _sender.Send(
                new ResolveReportCommand(
                    ReportId: id,
                    Note: request.Note,
                    Images: uploadFiles),
                cancellationToken);

            return Ok(result);
        }
        finally
        {
            foreach (var uploadFile in uploadFiles)
            {
                await uploadFile.Content.DisposeAsync();
            }
        }
    }
}
