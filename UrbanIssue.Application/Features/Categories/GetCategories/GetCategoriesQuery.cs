using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Categories.Common;

namespace UrbanIssue.Application.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery(
    string? Search = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<CategoryResult>>;
