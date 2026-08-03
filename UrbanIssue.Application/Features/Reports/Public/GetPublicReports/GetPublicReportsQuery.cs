using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Public.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReports;

public sealed record GetPublicReportsQuery(
    string? Search = null,
    int? CategoryId = null,
    int? AreaId = null,
    ReportStatus? Status = null,
    ReportPriority? Priority = null,
    ReportSortBy SortBy = ReportSortBy.Newest,
    decimal? CurrentLatitude = null,
    decimal? CurrentLongitude = null,

    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,

    decimal? MinLatitude = null,
    decimal? MaxLatitude = null,
    decimal? MinLongitude = null,
    decimal? MaxLongitude = null,

    int PageNumber = 1,
    int PageSize = 100)
    : IRequest<PagedResult<PublicReportMapItemResult>>;
