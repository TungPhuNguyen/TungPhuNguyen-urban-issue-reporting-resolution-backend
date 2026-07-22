using MediatR;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.GetMyReportById;

public sealed record GetMyReportByIdQuery(
    Guid Id)
    : IRequest<CitizenReportDetailResult>;
