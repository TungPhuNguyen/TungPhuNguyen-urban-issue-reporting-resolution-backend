using MediatR;
using UrbanIssue.Application.Features.Reports.Upvotes.Common;

namespace UrbanIssue.Application.Features.Reports.Upvotes.RemoveReportUpvote;

public sealed record RemoveReportUpvoteCommand(
    Guid ReportId)
    : IRequest<ReportUpvoteResult>;
