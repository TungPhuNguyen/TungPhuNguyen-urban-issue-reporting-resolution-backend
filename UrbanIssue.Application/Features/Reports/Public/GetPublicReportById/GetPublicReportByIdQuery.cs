using MediatR;
using UrbanIssue.Application.Features.Reports.Public.Common;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReportById;

public sealed record GetPublicReportByIdQuery(
    Guid ReportId)
    : IRequest<PublicReportDetailResult>;
