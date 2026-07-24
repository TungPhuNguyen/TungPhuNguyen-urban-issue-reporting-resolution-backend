using MediatR;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;

namespace UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;

public sealed record SubmitComplaintCommand(
    Guid ReportId,
    string Reason)
    : IRequest<PostResolutionActionResult>;
