using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Comments.Common;

namespace UrbanIssue.Application.Features.Reports.Comments.GetReportComments;

public sealed record GetReportCommentsQuery(
    Guid ReportId,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<ReportCommentResult>>;
