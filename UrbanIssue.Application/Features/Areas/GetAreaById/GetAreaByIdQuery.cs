using MediatR;
using UrbanIssue.Application.Features.Areas.Common;

namespace UrbanIssue.Application.Features.Areas.GetAreaById;

public sealed record GetAreaByIdQuery(
    int Id)
    : IRequest<AreaResult>;
