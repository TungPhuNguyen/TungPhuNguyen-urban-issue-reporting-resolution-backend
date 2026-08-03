using MediatR;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;

namespace UrbanIssue.Application.Features.Reports.CancelReport;

public sealed record CancelReportCommand(
    Guid ReportId,
    string Reason,
    byte[] RowVersion)
    : IRequest<PostResolutionActionResult>;
