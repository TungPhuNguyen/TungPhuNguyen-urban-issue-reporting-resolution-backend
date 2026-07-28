using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Staff.Common;

namespace UrbanIssue.Application.Features.Reports.Staff.UploadProgressImages;

public sealed record UploadProgressImagesCommand(
    Guid ReportId,
    string? Note,
    IReadOnlyList<UploadFile> Images)
    : IRequest<StaffProgressUpdateResult>;
