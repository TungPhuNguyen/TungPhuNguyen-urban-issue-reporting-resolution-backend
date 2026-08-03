using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Application.Features.Reports.GetMyReportById;

namespace UrbanIssue.Application.Features.Reports.GetMyReportByCode;

public sealed class GetMyReportByCodeQueryHandler
    : IRequestHandler<GetMyReportByCodeQuery, CitizenReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISender _sender;

    public GetMyReportByCodeQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        ISender sender)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _sender = sender;
    }

    public async Task<CitizenReportDetailResult> Handle(
        GetMyReportByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var code = request.ReportCode.Trim().ToUpperInvariant();
        var reportId = await _dbContext.Reports
            .AsNoTracking()
            .Where(report => report.ReportCode == code
                && report.CitizenId == _currentUserService.UserId)
            .Select(report => (Guid?)report.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!reportId.HasValue)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy phản ánh có mã {code}.");
        }

        return await _sender.Send(
            new GetMyReportByIdQuery(reportId.Value),
            cancellationToken);
    }
}
