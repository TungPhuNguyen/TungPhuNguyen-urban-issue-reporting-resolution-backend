using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

public sealed class ReportDuplicateChecker : IReportDuplicateChecker
{
    private const double SearchRadiusInMeters = 100d;
    private const double EarthRadiusInMeters = 6_371_000d;
    private const double MetersPerLatitudeDegree = 111_320d;
    private readonly IApplicationDbContext _dbContext;

    public ReportDuplicateChecker(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CheckDuplicateReportsResult> CheckAsync(
        int categoryId,
        decimal latitudeValue,
        decimal longitudeValue,
        CancellationToken cancellationToken,
        Guid? excludedReportId = null)
    {
        var latitude = (double)latitudeValue;
        var longitude = (double)longitudeValue;
        var latitudeDelta = SearchRadiusInMeters / MetersPerLatitudeDegree;
        var longitudeScale = MetersPerLatitudeDegree
            * Math.Abs(Math.Cos(DegreesToRadians(latitude)));
        var longitudeDelta = SearchRadiusInMeters
            / Math.Max(longitudeScale, 0.000001d);

        var minLatitude = (decimal)(latitude - latitudeDelta);
        var maxLatitude = (decimal)(latitude + latitudeDelta);
        var minLongitude = (decimal)(longitude - longitudeDelta);
        var maxLongitude = (decimal)(longitude + longitudeDelta);

        var candidates = await _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.CategoryId == categoryId
                && (!excludedReportId.HasValue
                    || report.Id != excludedReportId.Value)
                && report.Status != ReportStatus.Closed
                && report.Status != ReportStatus.Rejected
                && report.Status != ReportStatus.Cancelled
                && report.Latitude >= minLatitude
                && report.Latitude <= maxLatitude
                && report.Longitude >= minLongitude
                && report.Longitude <= maxLongitude)
            .Select(report => new Candidate(
                report.Id,
                report.ReportCode,
                report.Title,
                report.Description,
                report.Latitude,
                report.Longitude,
                report.Status,
                report.Upvotes.Count(),
                report.Images.OrderBy(image => image.Id)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault(),
                report.CreatedAt))
            .ToListAsync(cancellationToken);

        var reports = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Distance = CalculateDistanceInMeters(
                    latitude,
                    longitude,
                    (double)candidate.Latitude,
                    (double)candidate.Longitude)
            })
            .Where(item => item.Distance <= SearchRadiusInMeters)
            .OrderBy(item => item.Distance)
            .ThenByDescending(item => item.Candidate.CreatedAt)
            .Take(10)
            .Select(item => new DuplicateReportResult(
                item.Candidate.Id,
                item.Candidate.ReportCode,
                item.Candidate.Title,
                item.Candidate.Description,
                item.Candidate.Latitude,
                item.Candidate.Longitude,
                Math.Round(item.Distance, 1),
                item.Candidate.Status,
                item.Candidate.UpvoteCount,
                item.Candidate.ThumbnailUrl,
                item.Candidate.CreatedAt))
            .ToList();

        return new CheckDuplicateReportsResult(
            reports.Count > 0,
            SearchRadiusInMeters,
            reports);
    }

    private static double CalculateDistanceInMeters(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        var latitudeDifference = DegreesToRadians(latitude2 - latitude1);
        var longitudeDifference = DegreesToRadians(longitude2 - longitude1);
        var sineLatitude = Math.Sin(latitudeDifference / 2d);
        var sineLongitude = Math.Sin(longitudeDifference / 2d);
        var a = sineLatitude * sineLatitude
            + Math.Cos(DegreesToRadians(latitude1))
            * Math.Cos(DegreesToRadians(latitude2))
            * sineLongitude * sineLongitude;
        a = Math.Clamp(a, 0d, 1d);
        return EarthRadiusInMeters * 2d * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1d - a));
    }

    private static double DegreesToRadians(double degrees) =>
        degrees * Math.PI / 180d;

    private sealed record Candidate(
        Guid Id,
        string ReportCode,
        string Title,
        string Description,
        decimal Latitude,
        decimal Longitude,
        ReportStatus Status,
        int UpvoteCount,
        string? ThumbnailUrl,
        DateTime CreatedAt);
}
