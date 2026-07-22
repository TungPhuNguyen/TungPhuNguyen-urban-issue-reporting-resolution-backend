using MediatR;
using UrbanIssue.Application.Features.Reports.Upvotes.Common;

namespace UrbanIssue.Application.Features.Reports.Upvotes.AddReportUpvote;

public sealed record AddReportUpvoteCommand(
    Guid ReportId)
    : IRequest<ReportUpvoteResult>;
