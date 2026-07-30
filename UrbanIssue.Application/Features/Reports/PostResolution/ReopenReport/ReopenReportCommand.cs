using MediatR;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;

namespace UrbanIssue.Application.Features.Reports.PostResolution.ReopenReport;

public sealed record ReopenReportCommand(
    Guid ReportId,
    string Reason)
    : IRequest<PostResolutionActionResult>;
