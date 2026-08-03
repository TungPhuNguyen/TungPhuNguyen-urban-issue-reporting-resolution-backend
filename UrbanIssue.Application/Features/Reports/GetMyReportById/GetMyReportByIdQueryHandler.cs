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
        var currentTime = DateTime.UtcNow;

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
                    item.ReportNumber,
                    item.ReportCode,
                    item.Title,

                    item.CategoryId,
                    item.Category.Name,

                    item.AreaId,
                    item.Area.Name,

                    item.DepartmentId,
                    item.Department == null
                        ? null
                        : item.Department.Name,

                    item.Description,
                    item.OtherCategoryText,
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
                    item.ReopenReason,

                    item.Upvotes.Any(upvote =>
                        upvote.UserId == citizenId),

                    item.Complaints
                        .OrderByDescending(complaint => complaint.CreatedAt)
                        .Select(complaint => new ComplaintResult(
                            complaint.Id,
                            complaint.Status,
                            complaint.Reason,
                            complaint.AdminDecisionReason,
                            complaint.ResolvedByAdminId,
                            complaint.ResolvedByAdmin == null
                                ? null
                                : complaint.ResolvedByAdmin.FullName,
                            complaint.CreatedAt,
                            complaint.ResolvedAt,
                            complaint.Images
                                .OrderBy(image => image.Id)
                                .Select(image => image.ImageUrl)
                                .ToList()))
                        .FirstOrDefault(),

                    item.StatusUpdates
                        .Where(update =>
                            update.NewStatus == UrbanIssue.Domain.Enums.ReportStatus.Resolved)
                        .OrderByDescending(update => update.CreatedAt)
                        .Select(update => new ReportResolutionResult(
                            update.Note,
                            update.UpdatedByUserId,
                            update.UpdatedByUser == null
                                ? null
                                : update.UpdatedByUser.FullName,
                            update.CreatedAt,
                            update.Images
                                .OrderBy(image => image.Id)
                                .Select(image => image.ImageUrl)
                                .ToList()))
                        .FirstOrDefault(),

                    new ReportAllowedActionsResult(
                        item.Status == UrbanIssue.Domain.Enums.ReportStatus.New
                            || item.Status == UrbanIssue.Domain.Enums.ReportStatus.Assigned,
                        item.Status == UrbanIssue.Domain.Enums.ReportStatus.New
                            || item.Status == UrbanIssue.Domain.Enums.ReportStatus.Assigned,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        item.Status == UrbanIssue.Domain.Enums.ReportStatus.Resolved
                            && !item.Complaints.Any(complaint =>
                                complaint.Status == UrbanIssue.Domain.Enums.ComplaintStatus.Pending),
                        item.Status == UrbanIssue.Domain.Enums.ReportStatus.Resolved
                            && item.ResolvedAt.HasValue
                            && item.ResolvedAt.Value.AddDays(7) >= currentTime
                            && !item.Complaints.Any(),
                        false,
                        false),

                    item.RowVersion))
            .SingleOrDefaultAsync(cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.Id}.");
        }

        return report;
    }
}
