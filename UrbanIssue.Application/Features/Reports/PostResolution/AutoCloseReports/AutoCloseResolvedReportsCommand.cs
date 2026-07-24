using MediatR;

namespace UrbanIssue.Application.Features.Reports.PostResolution.AutoCloseReports;

public sealed record AutoCloseResolvedReportsCommand(
    DateTime CurrentTime)
    : IRequest<int>;
