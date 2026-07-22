using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.GetMyReportById;

public sealed class GetMyReportByIdQueryHandler
    : IRequestHandler<
        GetMyReportByIdQuery,
        CitizenReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    public GetMyReportByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<CitizenReportDetailResult> Handle(
        GetMyReportByIdQuery request,
        CancellationToken cancellationToken)
    {
        var citizenId =
            _currentUserService.UserId;

        /*
         * Lọc đồng thời theo ReportId và CitizenId.
         *
         * Citizen khác truy cập Report không thuộc mình
         * cũng nhận 404 để tránh làm lộ dữ liệu.
         */
        var report =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.Id == request.Id
                    && report.CitizenId == citizenId)
                .Select(report =>
                    new CitizenReportDetailResult(
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

                        report.RequiresManualAssignment,

                        report.Upvotes.Count(),

                        report.Images
                            .OrderBy(image => image.Id)
                            .Select(image =>
                                image.ImageUrl)
                            .ToList(),

                        report.AppliedSLAHours,
                        report.SLAStartedAt,
                        report.DueAt,

                        report.CreatedAt,
                        report.UpdatedAt,
                        report.AcceptedAt,
                        report.ResolvedAt,
                        report.ClosedAt,

                        report.RejectedAt,
                        report.RejectedReason,

                        report.ReopenedAt,
                        report.ReopenReason))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.Id}.");
        }

        return report;
    }
}
