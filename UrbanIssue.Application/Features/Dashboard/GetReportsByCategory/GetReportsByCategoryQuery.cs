using MediatR;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetReportsByCategory;

public sealed record GetReportsByCategoryQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IRequest<IReadOnlyList<ReportsByCategoryResult>>,
      IDashboardDateRangeQuery;
