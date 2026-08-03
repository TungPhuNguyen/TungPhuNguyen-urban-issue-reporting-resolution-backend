using MediatR;

namespace UrbanIssue.Application.Features.Areas.UpdateAreaBoundary;

public sealed record UpdateAreaBoundaryCommand(
    int AreaId,
    string? GeoJson)
    : IRequest<AreaBoundaryResult>;

public sealed record AreaBoundaryResult(
    int AreaId,
    string AreaName,
    bool HasBoundary,
    string? GeoJson,
    DateTime? UpdatedAt);
