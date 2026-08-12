using MediatR;
using UrbanIssue.Application.Features.Reports.Admin.Common;

namespace UrbanIssue.Application.Features.Reports.Admin.GetAdminReportById;

public sealed record GetAdminReportByIdQuery(
    Guid ReportId)
    : IRequest<AdminReportDetailResult>;
