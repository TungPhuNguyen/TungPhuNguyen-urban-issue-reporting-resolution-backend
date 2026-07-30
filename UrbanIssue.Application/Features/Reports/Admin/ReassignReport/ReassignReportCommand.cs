using MediatR;
using UrbanIssue.Application.Features.Reports.Admin.Common;

namespace UrbanIssue.Application.Features.Reports.Admin.ReassignReport;

public sealed record ReassignReportCommand(
    Guid ReportId,
    int DepartmentId,
    Guid? StaffId,
    string Reason)
    : IRequest<AdminReportActionResult>;
