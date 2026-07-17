using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Areas.Common;

namespace UrbanIssue.Application.Features.Areas.GetAreas;

public sealed record GetAreasQuery(
    string? Search = null,
    int? ParentAreaId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<AreaResult>>;
