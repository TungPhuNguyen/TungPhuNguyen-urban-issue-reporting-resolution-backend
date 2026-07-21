using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Reports;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.CreateReport;

namespace UrbanIssue.API.Controllers.V1.Citizen;

[ApiController]
[Route("api/v1/citizen/reports")]
[Authorize(Roles = "Citizen")]
[Produces("application/json")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(
        ISender sender)
    {
        _sender =
            sender;
    }

    /// <summary>
    /// Citizen tạo báo cáo sự cố mới.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(CreateReportResult),
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
    public async Task<ActionResult<CreateReportResult>>
        CreateReport(
            [FromForm] CreateReportRequest request,
            CancellationToken cancellationToken)
    {
        var uploadFiles =
            request.Images
                .Select(image =>
                    new UploadFile(
                        FileName:
                            image.FileName,

                        ContentType:
                            image.ContentType,

                        Length:
                            image.Length,

                        Content:
                            image.OpenReadStream()))
                .ToList();

        try
        {
            var command =
                new CreateReportCommand(
                    CategoryId:
                        request.CategoryId,

                    AreaId:
                        request.AreaId,

                    Description:
                        request.Description,

                    AddressText:
                        request.AddressText,

                    Latitude:
                        request.Latitude,

                    Longitude:
                        request.Longitude,

                    Images:
                        uploadFiles);

            var result =
                await _sender.Send(
                    command,
                    cancellationToken);

            return Created(
                $"/api/v1/citizen/reports/{result.Id}",
                result);
        }
        finally
        {
            foreach (var uploadFile in uploadFiles)
            {
                await uploadFile.Content
                    .DisposeAsync();
            }
        }
    }
}
