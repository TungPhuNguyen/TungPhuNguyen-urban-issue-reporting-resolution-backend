using MediatR;
using UrbanIssue.Application.Features.Areas.Common;

namespace UrbanIssue.Application.Features.Areas.UpdateArea;

public sealed record UpdateAreaCommand(
    int Id,
    string Name,
    string Code,
    int? ParentAreaId,
    bool IsActive)
    : IRequest<AreaResult>;
