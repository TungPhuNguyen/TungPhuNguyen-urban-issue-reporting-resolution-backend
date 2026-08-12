using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Staff.Common;

namespace UrbanIssue.Application.Features.Reports.Staff.ResolveReport;

public sealed record ResolveReportCommand(
    Guid ReportId,
    string Note,
    IReadOnlyList<UploadFile> Images)
    : IRequest<StaffReportActionResult>;
