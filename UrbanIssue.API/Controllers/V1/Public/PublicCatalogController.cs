using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Features.PublicCatalog.Common;
using UrbanIssue.Application.Features.PublicCatalog.GetPublicAreas;
using UrbanIssue.Application.Features.PublicCatalog.GetPublicCategories;

namespace UrbanIssue.API.Controllers.V1.Public;

[ApiController]
[Route("api/v1/public")]
[AllowAnonymous]
[Produces("application/json")]
public sealed class PublicCatalogController : ControllerBase
{
    private readonly ISender _sender;

    public PublicCatalogController(
        ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lấy danh sách loại sự cố đang hoạt động.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(
        typeof(IReadOnlyList<PublicCategoryResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyList<PublicCategoryResult>>>
        GetCategories(
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetPublicCategoriesQuery(),
                cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách khu vực đang hoạt động.
    /// </summary>
    [HttpGet("areas")]
    [ProducesResponseType(
        typeof(IReadOnlyList<PublicAreaResult>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<IReadOnlyList<PublicAreaResult>>>
        GetAreas(
            [FromQuery] GetPublicAreasQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }
}
