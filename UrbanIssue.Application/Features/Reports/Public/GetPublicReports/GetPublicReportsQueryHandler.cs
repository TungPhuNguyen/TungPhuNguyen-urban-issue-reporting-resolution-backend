using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Public.Common;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReports;

public sealed class GetPublicReportsQueryHandler
    : IRequestHandler<
        GetPublicReportsQuery,
        PagedResult<PublicReportMapItemResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPublicReportsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<PublicReportMapItemResult>> Handle(
        GetPublicReportsQuery request,
        CancellationToken cancellationToken)
    {
        /*
         * Rejected luôn bị loại khỏi dữ liệu công khai,
         * kể cả khi không truyền status.
         */
        var query = _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.Status != ReportStatus.Rejected
                && report.Status != ReportStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(report =>
                report.ReportCode.Contains(search)
                || report.Title.Contains(search)
                || report.Description.Contains(search)
                || report.Category.Name.Contains(search)
                || report.Area.Name.Contains(search));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(report =>
                report.CategoryId
                    == request.CategoryId.Value);
        }

        if (request.AreaId.HasValue)
        {
            query = query.Where(report =>
                report.AreaId
                    == request.AreaId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(report =>
                report.Status
                    == request.Status.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(report =>
                report.Priority == request.Priority.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(report =>
                report.CreatedAt
                    >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(report =>
                report.CreatedAt
                    <= request.CreatedTo.Value);
        }

        /*
         * Lọc theo bounding box của bản đồ.
         * Việc này được SQL Server thực hiện trước khi trả dữ liệu.
         */
        if (request.MinLatitude.HasValue
            && request.MaxLatitude.HasValue
            && request.MinLongitude.HasValue
            && request.MaxLongitude.HasValue)
        {
            query = query.Where(report =>
                report.Latitude
                    >= request.MinLatitude.Value
                && report.Latitude
                    <= request.MaxLatitude.Value
                && report.Longitude
                    >= request.MinLongitude.Value
                && report.Longitude
                    <= request.MaxLongitude.Value);
        }

        var totalItems =
            await query.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var orderedQuery = request.SortBy switch
        {
            ReportSortBy.MostUpvoted => query
                .OrderByDescending(report => report.Upvotes.Count())
                .ThenByDescending(report => report.CreatedAt),
            ReportSortBy.Nearby => query
                .OrderBy(report =>
                    (report.Latitude - request.CurrentLatitude!.Value)
                    * (report.Latitude - request.CurrentLatitude.Value)
                    + (report.Longitude - request.CurrentLongitude!.Value)
                    * (report.Longitude - request.CurrentLongitude.Value))
                .ThenByDescending(report => report.CreatedAt),
            _ => query.OrderByDescending(report => report.CreatedAt)
        };

        var currentUserId = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : (Guid?)null;

        var items = await orderedQuery
            .ThenByDescending(report =>
                report.Id)
            .Skip(
                (request.PageNumber - 1)
                * request.PageSize)
            .Take(request.PageSize)
            .Select(report =>
                new PublicReportMapItemResult(
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
                        .FirstOrDefault(),

                    report.CreatedAt,
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
            .ToListAsync(
                cancellationToken);

        return new PagedResult<PublicReportMapItemResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
