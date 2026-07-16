using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Features.Categories.Common;
using UrbanIssue.Application.Features.Categories.CreateCategory;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Categories.GetCategories;
using UrbanIssue.Application.Features.Categories.GetCategoryById;
using UrbanIssue.API.Contracts.Categories;
using UrbanIssue.Application.Features.Categories.UpdateCategory;
using UrbanIssue.Application.Features.Categories.DeleteCategory;




namespace UrbanIssue.API.Controllers.V1.Admin;

[ApiController]
[Route("api/v1/admin/categories")]
[Authorize(Roles = "Admin")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(
        ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Tạo loại sự cố và các cấu hình SLA mặc định.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(CategoryResult),
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
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResult>>
        CreateCategory(
            [FromBody] CreateCategoryCommand command,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Created(
            $"/api/v1/admin/categories/{result.Id}",
            result);
    }
    /// <summary>
    /// Lấy danh sách loại sự cố dành cho Admin.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<CategoryResult>),
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
        ActionResult<PagedResult<CategoryResult>>>
        GetCategories(
            [FromQuery] GetCategoriesQuery query,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Lấy chi tiết loại sự cố theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(CategoryResult),
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
    public async Task<ActionResult<CategoryResult>>
        GetCategoryById(
            int id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new GetCategoryByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Cập nhật loại sự cố.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(CategoryResult),
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
    public async Task<ActionResult<CategoryResult>>
        UpdateCategory(
            int id,
            [FromBody] UpdateCategoryRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateCategoryCommand(
                Id: id,
                Name: request.Name,
                Description: request.Description,
                IsActive: request.IsActive);

        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(result);
    }
    /// <summary>
    /// Ngừng hoạt động một loại sự cố.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
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
    public async Task<IActionResult>
        DeleteCategory(
            int id,
            CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteCategoryCommand(id),
            cancellationToken);

        return NoContent();
    }
}
