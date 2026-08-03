using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Public.Common;
using UrbanIssue.Application.Features.Reports.Public.GetPublicReportById;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReportByCode;

public sealed class GetPublicReportByCodeQueryHandler
    : IRequestHandler<GetPublicReportByCodeQuery, PublicReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISender _sender;

    public GetPublicReportByCodeQueryHandler(
        IApplicationDbContext dbContext,
        ISender sender)
    {
        _dbContext = dbContext;
        _sender = sender;
    }

    public async Task<PublicReportDetailResult> Handle(
        GetPublicReportByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var normalizedCode = request.ReportCode.Trim().ToUpperInvariant();
        var reportId = await _dbContext.Reports
            .AsNoTracking()
            .Where(report => report.ReportCode == normalizedCode)
            .Select(report => (Guid?)report.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!reportId.HasValue)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy phản ánh có mã {normalizedCode}.");
        }

        return await _sender.Send(
            new GetPublicReportByIdQuery(reportId.Value),
            cancellationToken);
    }
}
