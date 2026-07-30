using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Public.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReportById;

public sealed class GetPublicReportByIdQueryHandler
    : IRequestHandler<
        GetPublicReportByIdQuery,
        PublicReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPublicReportByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PublicReportDetailResult> Handle(
        GetPublicReportByIdQuery request,
        CancellationToken cancellationToken)
    {
        /*
         * Rejected được trả 404 để không công khai dữ liệu.
         */
        var report = await _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.Id == request.ReportId
                && report.Status
                    != ReportStatus.Rejected)
            .Select(report =>
                new PublicReportDetailResult(
                    report.Id,

                    report.CategoryId,
                    report.Category.Name,

                    report.AreaId,
                    report.Area.Name,

                    report.DepartmentId,
                    report.Department == null
                        ? null
                        : report.Department.Name,

                    report.Description,
                    report.AddressText,

                    report.Latitude,
                    report.Longitude,

                    report.Priority,
                    report.Status,

                    report.Upvotes.Count(),
                    report.Comments.Count(),

                    report.Images
                        .OrderBy(image => image.Id)
                        .Select(image =>
                            image.ImageUrl)
                        .ToList(),

                    report.CreatedAt,
                    report.UpdatedAt,
                    report.AcceptedAt,
                    report.ResolvedAt,
                    report.ClosedAt))
            .SingleOrDefaultAsync(
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo công khai có ID "
                + $"{request.ReportId}.");
        }

        return report;
    }
}
