using MediatR;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;

namespace UrbanIssue.Application.Features.Reports.PostResolution.CloseReport;

public sealed record CloseReportCommand(
    Guid ReportId,
    string? Note)
    : IRequest<PostResolutionActionResult>;
