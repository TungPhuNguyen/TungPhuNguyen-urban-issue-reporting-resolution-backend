using MediatR;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Features.PublicCatalog.ResolveAreaByCoordinates;

public sealed record ResolveAreaByCoordinatesQuery(
    decimal Latitude,
    decimal Longitude)
    : IRequest<AreaLocationMatch>;
