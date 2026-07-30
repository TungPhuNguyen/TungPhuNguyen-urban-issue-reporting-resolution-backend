using MediatR;
using UrbanIssue.Application.Features.Reports.Staff.Common;

namespace UrbanIssue.Application.Features.Reports.Staff.GetStaffReportById;

public sealed record GetStaffReportByIdQuery(
    Guid ReportId)
    : IRequest<StaffReportDetailResult>;
