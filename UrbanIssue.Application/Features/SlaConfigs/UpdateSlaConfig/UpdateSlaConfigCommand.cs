using MediatR;
using UrbanIssue.Application.Features.SlaConfigs.Common;

namespace UrbanIssue.Application.Features.SlaConfigs.UpdateSlaConfig;

public sealed record UpdateSlaConfigCommand(
    int Id,
    int DurationHours)
    : IRequest<SlaConfigResult>;
