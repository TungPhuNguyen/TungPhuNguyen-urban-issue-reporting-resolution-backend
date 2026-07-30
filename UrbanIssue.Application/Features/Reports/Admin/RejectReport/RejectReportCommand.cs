using MediatR;
using UrbanIssue.Application.Features.Reports.Admin.Common;

namespace UrbanIssue.Application.Features.Reports.Admin.RejectReport;

public sealed record RejectReportCommand(
    Guid ReportId,
    string Reason)
    : IRequest<AdminReportActionResult>;
