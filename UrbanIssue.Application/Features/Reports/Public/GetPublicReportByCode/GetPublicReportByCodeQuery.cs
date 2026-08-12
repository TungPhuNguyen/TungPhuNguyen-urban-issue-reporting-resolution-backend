using MediatR;
using UrbanIssue.Application.Features.Reports.Public.Common;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReportByCode;

public sealed record GetPublicReportByCodeQuery(string ReportCode)
    : IRequest<PublicReportDetailResult>;
