using MediatR;

namespace UrbanIssue.Application.Features.Reports.GetReportTimeline;

public sealed record GetReportTimelineQuery(
    Guid ReportId)
    : IRequest<GetReportTimelineResult>;
