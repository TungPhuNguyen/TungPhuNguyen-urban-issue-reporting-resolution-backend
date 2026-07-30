using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

public sealed class CheckDuplicateReportsCommandHandler
    : IRequestHandler<
        CheckDuplicateReportsCommand,
        CheckDuplicateReportsResult>
{
    private const double SearchRadiusInMeters =
        100d;

    private const double EarthRadiusInMeters =
        6_371_000d;

    private const double MetersPerLatitudeDegree =
        111_320d;

    private readonly IApplicationDbContext
        _dbContext;

    public CheckDuplicateReportsCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CheckDuplicateReportsResult> Handle(
        CheckDuplicateReportsCommand request,
        CancellationToken cancellationToken)
    {
        var category =
            await _dbContext.Categories
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    category =>
                        category.Id == request.CategoryId,
                    cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại sự cố có ID {request.CategoryId}.");
        }

        if (!category.IsActive)
        {
            throw new ConflictException(
                "Không thể kiểm tra báo cáo trùng với loại sự cố đã ngừng hoạt động.");
        }

        var latitude =
            (double)request.Latitude;

        var longitude =
            (double)request.Longitude;

        /*
         * Tính vùng chữ nhật bao quanh bán kính 100 mét.
         * Sau đó mới dùng Haversine để tính chính xác.
         */
        var latitudeDelta =
            SearchRadiusInMeters
            / MetersPerLatitudeDegree;

        var latitudeRadians =
            DegreesToRadians(latitude);

        var longitudeScale =
            MetersPerLatitudeDegree
            * Math.Abs(Math.Cos(latitudeRadians));

        /*
         * Tránh phép chia cho số gần 0
         * khi tọa độ ở gần hai cực.
         */
        longitudeScale =
            Math.Max(longitudeScale, 0.000001d);

        var longitudeDelta =
            SearchRadiusInMeters
            / longitudeScale;

        var minimumLatitude =
            (decimal)(latitude - latitudeDelta);

        var maximumLatitude =
            (decimal)(latitude + latitudeDelta);

        var minimumLongitude =
            (decimal)(longitude - longitudeDelta);

        var maximumLongitude =
            (decimal)(longitude + longitudeDelta);

        /*
         * Chỉ lấy:
         * - Cùng Category
         * - Chưa Closed
         * - Chưa Rejected
         * - Nằm trong bounding box
         */
        var candidates =
            await _dbContext.Reports
                .AsNoTracking()
                .Where(report =>
                    report.CategoryId
                        == request.CategoryId

                    && report.Status
                        != ReportStatus.Closed

                    && report.Status
                        != ReportStatus.Rejected

                    && report.Latitude
                        >= minimumLatitude

                    && report.Latitude
                        <= maximumLatitude

                    && report.Longitude
                        >= minimumLongitude

                    && report.Longitude
                        <= maximumLongitude)
                .Select(report =>
                    new DuplicateCandidate(
                        report.Id,
                        report.Description,
                        report.Latitude,
                        report.Longitude,
                        report.Status,
                        report.Upvotes.Count(),
                        report.Images
                            .OrderBy(image => image.Id)
                            .Select(image =>
                                image.ImageUrl)
                            .FirstOrDefault(),
                        report.CreatedAt))
                .ToListAsync(
                    cancellationToken);

        /*
         * EF Core chỉ lọc bounding box.
         * Khoảng cách chính xác được tính trong bộ nhớ.
         */
        var duplicateReports =
            candidates
                .Select(candidate =>
                {
                    var distance =
                        CalculateDistanceInMeters(
                            latitude,
                            longitude,
                            (double)candidate.Latitude,
                            (double)candidate.Longitude);

                    return new
                    {
                        Candidate =
                            candidate,

                        Distance =
                            distance
                    };
                })
                .Where(result =>
                    result.Distance
                        <= SearchRadiusInMeters)
                .OrderBy(result =>
                    result.Distance)
                .ThenByDescending(result =>
                    result.Candidate.CreatedAt)
                .Take(10)
                .Select(result =>
                    new DuplicateReportResult(
                        Id:
                            result.Candidate.Id,

                        Description:
                            result.Candidate.Description,

                        Latitude:
                            result.Candidate.Latitude,

                        Longitude:
                            result.Candidate.Longitude,

                        DistanceInMeters:
                            Math.Round(
                                result.Distance,
                                1),

                        Status:
                            result.Candidate.Status,

                        UpvoteCount:
                            result.Candidate.UpvoteCount,

                        ThumbnailUrl:
                            result.Candidate.ThumbnailUrl,

                        CreatedAt:
                            result.Candidate.CreatedAt))
                .ToList();

        return new CheckDuplicateReportsResult(
            HasPossibleDuplicates:
                duplicateReports.Count > 0,

            SearchRadiusInMeters:
                SearchRadiusInMeters,

            Reports:
                duplicateReports);
    }

    private static double CalculateDistanceInMeters(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        var latitudeDifference =
            DegreesToRadians(
                latitude2 - latitude1);

        var longitudeDifference =
            DegreesToRadians(
                longitude2 - longitude1);

        var latitude1Radians =
            DegreesToRadians(latitude1);

        var latitude2Radians =
            DegreesToRadians(latitude2);

        var sineLatitude =
            Math.Sin(
                latitudeDifference / 2d);

        var sineLongitude =
            Math.Sin(
                longitudeDifference / 2d);

        var a =
            sineLatitude * sineLatitude
            + Math.Cos(latitude1Radians)
            * Math.Cos(latitude2Radians)
            * sineLongitude
            * sineLongitude;

        /*
         * Chống sai số floating point khiến a
         * nhỏ hơn 0 hoặc lớn hơn 1.
         */
        a = Math.Clamp(
            a,
            0d,
            1d);

        var centralAngle =
            2d * Math.Atan2(
                Math.Sqrt(a),
                Math.Sqrt(1d - a));

        return EarthRadiusInMeters
            * centralAngle;
    }

    private static double DegreesToRadians(
        double degrees)
    {
        return degrees
            * Math.PI
            / 180d;
    }

    private sealed record DuplicateCandidate(
        Guid Id,
        string Description,
        decimal Latitude,
        decimal Longitude,
        ReportStatus Status,
        int UpvoteCount,
        string? ThumbnailUrl,
        DateTime CreatedAt);
}
