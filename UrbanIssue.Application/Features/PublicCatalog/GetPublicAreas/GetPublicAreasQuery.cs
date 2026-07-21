using MediatR;
using UrbanIssue.Application.Features.PublicCatalog.Common;

namespace UrbanIssue.Application.Features.PublicCatalog.GetPublicAreas;

public sealed record GetPublicAreasQuery(
    int? ParentAreaId = null)
    : IRequest<IReadOnlyList<PublicAreaResult>>;
