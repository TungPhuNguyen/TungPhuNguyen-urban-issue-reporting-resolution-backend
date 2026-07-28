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
    private readonly ICurrentUserService _currentUserService;

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
        var citizenId = _currentUserService.UserId;

        /*
         * Lọc đồng thời theo ReportId và CitizenId.
         * Citizen khác truy cập Report không thuộc mình
         * cũng nhận 404 để tránh làm lộ dữ liệu.
         */
        var report = await _dbContext.Reports
            .AsNoTracking()
            .Where(item =>
                item.Id == request.Id
                && item.CitizenId == citizenId)
            .Select(item =>
                new CitizenReportDetailResult(
                    item.Id,

                    item.CategoryId,
                    item.Category.Name,

                    item.AreaId,
                    item.Area.Name,

                    item.DepartmentId,
                    item.Department == null
                        ? null
                        : item.Department.Name,

                    item.Description,
                    item.AddressText,

                    item.Latitude,
                    item.Longitude,

                    item.Priority,
                    item.Status,

                    item.RequiresManualAssignment,
                    item.Upvotes.Count(),

                    item.Images
                        .OrderBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .ToList(),

                    item.AppliedSLAHours,
                    item.SLAStartedAt,
                    item.DueAt,

                    item.CreatedAt,
                    item.UpdatedAt,
                    item.AcceptedAt,
                    item.ResolvedAt,
                    item.ClosedAt,

                    item.HasSubmittedComplaint,
                    item.ComplaintSubmittedAt,
                    item.ComplaintReason,

                    item.RejectedAt,
                    item.RejectedReason,

                    item.ReopenedAt,
                    item.ReopenReason))
            .SingleOrDefaultAsync(cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.Id}.");
        }

        return report;
    }
}
