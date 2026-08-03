using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Features.Reports.Public.Common;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReportById;

public sealed class GetPublicReportByIdQueryHandler
    : IRequestHandler<
        GetPublicReportByIdQuery,
        PublicReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPublicReportByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PublicReportDetailResult> Handle(
        GetPublicReportByIdQuery request,
        CancellationToken cancellationToken)
    {
        /*
         * Rejected được trả 404 để không công khai dữ liệu.
         */
        var currentUserId = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : (Guid?)null;

        var report = await _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.Id == request.ReportId
                && report.Status
                    != ReportStatus.Rejected
                && report.Status != ReportStatus.Cancelled)
            .Select(report =>
                new PublicReportDetailResult(
                    report.Id,
                    report.ReportCode,
                    report.Title,

                    report.CategoryId,
                    report.Category.Name,

                    report.AreaId,
                    report.Area.Name,

                    report.DepartmentId,
                    report.Department == null
                        ? null
                        : report.Department.Name,

                    report.Description,
                    report.Area.Name,

                    Math.Round(report.Latitude, 3),
                    Math.Round(report.Longitude, 3),

                    report.Priority,
                    report.Status,

                    report.Upvotes.Count(),
                    currentUserId.HasValue
                        && report.Upvotes.Any(upvote =>
                            upvote.UserId == currentUserId.Value),
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
                    report.ClosedAt,
                    new ReportAllowedActionsResult(
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        currentUserId.HasValue
                            && report.CitizenId != currentUserId.Value
                            && report.Status != ReportStatus.Closed
                            && report.Status != ReportStatus.Rejected
                            && report.Status != ReportStatus.Cancelled)))
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
