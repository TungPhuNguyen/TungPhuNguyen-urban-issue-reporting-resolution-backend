using MediatR;

namespace UrbanIssue.Application.Features.Reports.SlaMonitoring;

public sealed record ProcessSlaMonitoringCommand(
    DateTime CurrentTime)
    : IRequest<ProcessSlaMonitoringResult>;
