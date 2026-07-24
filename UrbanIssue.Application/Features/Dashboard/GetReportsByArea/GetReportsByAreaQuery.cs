using MediatR;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetReportsByArea;

public sealed record GetReportsByAreaQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IRequest<IReadOnlyList<ReportsByAreaResult>>,
      IDashboardDateRangeQuery;
