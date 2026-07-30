using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Admin.Common;

namespace UrbanIssue.Application.Features.Reports.Admin.GetAdminReportById;

public sealed class GetAdminReportByIdQueryHandler
    : IRequestHandler<
        GetAdminReportByIdQuery,
        AdminReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdminReportByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminReportDetailResult> Handle(
        GetAdminReportByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.Id == request.ReportId)
            .Select(report =>
                new AdminReportDetailResult(
                    report.Id,

                    report.CitizenId,
                    report.Citizen.FullName,
                    report.Citizen.Email,

                    report.CategoryId,
                    report.Category.Name,

                    report.AreaId,
                    report.Area.Name,

                    report.DepartmentId,
                    report.Department == null
                        ? null
                        : report.Department.Name,

                    report.AssignedStaffId,
                    report.AssignedStaff == null
                        ? null
                        : report.AssignedStaff.FullName,

                    report.Description,
                    report.AddressText,
                    report.Latitude,
                    report.Longitude,

                    report.Priority,
                    report.Status,
                    report.RequiresManualAssignment,

                    report.Upvotes.Count(),
                    report.Comments.Count(),

                    report.Images
                        .OrderBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .ToList(),

                    report.SLAConfigId,
                    report.AppliedSLAHours,
                    report.SLAStartedAt,
                    report.DueAt,

                    report.IsEscalated,
                    report.EscalatedAt,

                    report.CreatedAt,
                    report.UpdatedAt,
                    report.AcceptedAt,
                    report.ResolvedAt,
                    report.ClosedAt,

                    report.HasSubmittedComplaint,
                    report.ComplaintSubmittedAt,
                    report.ComplaintReason,

                    report.RejectedAt,
                    report.RejectedReason,

                    report.ReopenedAt,
                    report.ReopenReason))
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        return result;
    }
}
