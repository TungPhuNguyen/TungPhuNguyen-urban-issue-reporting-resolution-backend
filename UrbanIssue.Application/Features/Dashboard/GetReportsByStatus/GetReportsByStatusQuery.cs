using MediatR;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetReportsByStatus;

public sealed record GetReportsByStatusQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IRequest<IReadOnlyList<ReportsByStatusResult>>,
      IDashboardDateRangeQuery;
