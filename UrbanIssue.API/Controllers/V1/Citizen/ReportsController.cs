using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Contracts.Reports;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.CreateReport;
using UrbanIssue.Application.Features.Reports.CheckDuplicateReports;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Application.Features.Reports.GetMyReportById;
using UrbanIssue.Application.Features.Reports.GetMyReports;
using UrbanIssue.Application.Features.Reports.GetReportTimeline;
using UrbanIssue.Application.Features.Reports.Upvotes.AddReportUpvote;
using UrbanIssue.Application.Features.Reports.Upvotes.Common;
using UrbanIssue.Application.Features.Reports.Upvotes.RemoveReportUpvote;
using UrbanIssue.Application.Features.Reports.Comments.AddReportComment;
using UrbanIssue.Application.Features.Reports.Comments.Common;
using UrbanIssue.Application.Features.Reports.Comments.DeleteReportComment;
using UrbanIssue.Application.Features.Reports.Comments.GetReportComments;
using UrbanIssue.Application.Features.Reports.PostResolution.CloseReport;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;




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
    /// Kiểm tra các báo cáo có khả năng trùng trong bán kính 100 mét.
    /// </summary>
    [HttpPost("check-duplicates")]
    [Consumes("application/json")]
    [ProducesResponseType(
        typeof(CheckDuplicateReportsResult),
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
    public async Task<ActionResult<CheckDuplicateReportsResult>>
        CheckDuplicateReports(
            [FromBody]
        CheckDuplicateReportsRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new CheckDuplicateReportsCommand(
                CategoryId:
                    request.CategoryId,

                Latitude:
                    request.Latitude,

                Longitude:
                    request.Longitude);

        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(result);
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
    /// <summary>
    /// Lấy danh sách báo cáo của Citizen đang đăng nhập.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<CitizenReportSummaryResult>),
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
        ActionResult<PagedResult<CitizenReportSummaryResult>>>
        GetMyReports(
            [FromQuery] GetMyReportsQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Lấy chi tiết một báo cáo thuộc Citizen đang đăng nhập.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(CitizenReportDetailResult),
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
    public async Task<ActionResult<CitizenReportDetailResult>>
        GetMyReportById(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetMyReportByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Lấy lịch sử thay đổi trạng thái của báo cáo.
    /// </summary>
    [HttpGet("{id:guid}/timeline")]
    [ProducesResponseType(
        typeof(GetReportTimelineResult),
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
    public async Task<ActionResult<GetReportTimelineResult>>
        GetReportTimeline(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetReportTimelineQuery(id),
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Upvote một báo cáo của Citizen khác.
    /// </summary>
    [HttpPost("{id:guid}/upvote")]
    [ProducesResponseType(
        typeof(ReportUpvoteResult),
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
    public async Task<ActionResult<ReportUpvoteResult>>
        AddUpvote(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new AddReportUpvoteCommand(id),
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Bỏ upvote khỏi một báo cáo.
    /// </summary>
    [HttpDelete("{id:guid}/upvote")]
    [ProducesResponseType(
        typeof(ReportUpvoteResult),
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
    public async Task<ActionResult<ReportUpvoteResult>>
        RemoveUpvote(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new RemoveReportUpvoteCommand(id),
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Lấy danh sách bình luận của một báo cáo.
    /// </summary>
    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(
        typeof(PagedResult<ReportCommentResult>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<PagedResult<ReportCommentResult>>>
        GetComments(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
    {
        var result =
            await _sender.Send(
                new GetReportCommentsQuery(
                    ReportId: id,
                    PageNumber: pageNumber,
                    PageSize: pageSize),
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Thêm bình luận vào một báo cáo.
    /// </summary>
    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(
        typeof(ReportCommentResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReportCommentResult>>
        AddComment(
            Guid id,
            [FromBody] AddReportCommentRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new AddReportCommentCommand(
                    ReportId: id,
                    Content: request.Content),
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
    /// <summary>
    /// Xóa bình luận do Citizen hiện tại tạo.
    /// </summary>
    [HttpDelete("{reportId:guid}/comments/{commentId:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult>
        DeleteComment(
            Guid reportId,
            int commentId,
            CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteReportCommentCommand(
                ReportId: reportId,
                CommentId: commentId),
            cancellationToken);

        return NoContent();
    }
    [HttpPost("{id:guid}/complaints")]
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
    SubmitComplaint(
        Guid id,
        [FromBody] SubmitComplaintRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SubmitComplaintCommand(
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
    CloseReport(
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
}
