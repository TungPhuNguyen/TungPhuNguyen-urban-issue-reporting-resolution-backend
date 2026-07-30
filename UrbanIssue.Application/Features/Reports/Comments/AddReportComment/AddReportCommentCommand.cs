using MediatR;
using UrbanIssue.Application.Features.Reports.Comments.Common;

namespace UrbanIssue.Application.Features.Reports.Comments.AddReportComment;

public sealed record AddReportCommentCommand(
    Guid ReportId,
    string Content)
    : IRequest<ReportCommentResult>;
