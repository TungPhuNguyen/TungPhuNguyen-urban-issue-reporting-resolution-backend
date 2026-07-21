using MediatR;

namespace UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

public sealed record CheckDuplicateReportsCommand(
    int CategoryId,
    decimal Latitude,
    decimal Longitude)
    : IRequest<CheckDuplicateReportsResult>;
