using MediatR;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;

namespace UrbanIssue.Application.Features.Reports.PostResolution.DismissComplaint;

public sealed record DismissComplaintCommand(
    Guid ReportId,
    string Reason)
    : IRequest<PostResolutionActionResult>;
