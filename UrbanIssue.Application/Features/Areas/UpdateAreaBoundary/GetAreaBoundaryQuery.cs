using MediatR;

namespace UrbanIssue.Application.Features.Areas.UpdateAreaBoundary;

public sealed record GetAreaBoundaryQuery(int AreaId)
    : IRequest<AreaBoundaryResult>;
