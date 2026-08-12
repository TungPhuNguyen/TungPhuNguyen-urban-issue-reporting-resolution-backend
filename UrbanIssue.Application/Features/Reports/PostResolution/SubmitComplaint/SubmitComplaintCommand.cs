using MediatR;
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;

public sealed record SubmitComplaintCommand(
    Guid ReportId,
    string Reason,
    IReadOnlyList<UploadFile> Images)
    : IRequest<PostResolutionActionResult>;
