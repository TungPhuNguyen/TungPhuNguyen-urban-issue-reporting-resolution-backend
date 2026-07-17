using MediatR;
using UrbanIssue.Application.Features.Areas.Common;

namespace UrbanIssue.Application.Features.Areas.CreateArea;

public sealed record CreateAreaCommand(
    string Name,
    string Code,
    int? ParentAreaId)
    : IRequest<AreaResult>;
