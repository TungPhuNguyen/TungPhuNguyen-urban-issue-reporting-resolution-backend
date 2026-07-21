using MediatR;
using UrbanIssue.Application.Features.SlaConfigs.Common;

namespace UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigById;

public sealed record GetSlaConfigByIdQuery(
    int Id)
    : IRequest<SlaConfigResult>;
