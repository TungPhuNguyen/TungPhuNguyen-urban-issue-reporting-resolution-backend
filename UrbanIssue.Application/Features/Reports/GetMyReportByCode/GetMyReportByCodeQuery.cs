using MediatR;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.GetMyReportByCode;

public sealed record GetMyReportByCodeQuery(string ReportCode)
    : IRequest<CitizenReportDetailResult>;
