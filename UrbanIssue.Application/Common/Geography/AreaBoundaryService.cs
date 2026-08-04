using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Geography;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Common.Geography;

public sealed class AreaBoundaryService
    : IAreaBoundaryService
{
    /*
     * Chỉ dùng fallback khi không polygon nào chứa chính xác điểm.
     * 3 km phù hợp dữ liệu demo của các phường nội thành.
     */
    private const double MaximumFallbackDistanceMeters = 3_000;

    private const double EarthRadiusMeters = 6_371_000;

    private readonly IApplicationDbContext _dbContext;

    public AreaBoundaryService(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AreaBoundaryCheckResult> CheckAsync(
        int areaId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken)
    {
        var boundary = await _dbContext.Areas
            .AsNoTracking()
            .Where(area => area.Id == areaId)
            .Select(area => area.BoundaryGeoJson)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(boundary))
        {
            return new AreaBoundaryCheckResult(
                HasBoundary: false,
                ContainsPoint: false);
        }

        try
        {
            return new AreaBoundaryCheckResult(
                HasBoundary: true,
                ContainsPoint: GeoJsonBoundary.Contains(
                    boundary,
                    latitude,
                    longitude));
        }
        catch (JsonException)
        {
            /*
             * Khu vực có boundary nhưng JSON không hợp lệ.
             * Không làm API trả lỗi 500.
             */
            return new AreaBoundaryCheckResult(
                HasBoundary: true,
                ContainsPoint: false);
        }
    }

    public async Task<AreaLocationMatch?> FindContainingWardAsync(
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken)
    {
        var candidates = await _dbContext.Areas
            .AsNoTracking()
            .Where(area =>
                area.IsActive
                && area.ParentAreaId.HasValue
                && area.BoundaryGeoJson != null
                && area.BoundaryGeoJson != "")
            .Select(area => new
            {
                area.Id,
                area.Name,
                area.Code,
                area.ParentAreaId,

                DistrictName =
                    area.ParentArea!.Name,

                area.BoundaryGeoJson
            })
            .ToListAsync(cancellationToken);

        /*
         * Bước 1: tìm polygon chứa chính xác tọa độ.
         */
        foreach (var area in candidates)
        {
            try
            {
                if (!GeoJsonBoundary.Contains(
                        area.BoundaryGeoJson!,
                        latitude,
                        longitude))
                {
                    continue;
                }

                return new AreaLocationMatch(
                    area.Id,
                    area.Name,
                    area.Code,
                    area.ParentAreaId!.Value,
                    area.DistrictName);
            }
            catch (JsonException)
            {
                /*
                 * Boundary của một phường bị lỗi thì bỏ qua
                 * và tiếp tục kiểm tra các phường khác.
                 */
            }
        }

        /*
         * Bước 2: dữ liệu OSM có thể bị hở hoặc polygon nhỏ.
         * Tìm boundary gần tọa độ nhất để phục vụ demo.
         */
        var fallback = candidates
            .Select(area => new
            {
                Area = area,

                Proximity = TryGetBoundaryProximity(
                    area.BoundaryGeoJson!,
                    latitude,
                    longitude)
            })
            .Where(item =>
                item.Proximity.HasValue
                && item.Proximity.Value.DistanceMeters
                    <= MaximumFallbackDistanceMeters)
            .OrderBy(item =>
                item.Proximity!.Value.DistanceMeters)
            .ThenBy(item =>
                item.Proximity!.Value.BoundingBoxArea)
            .FirstOrDefault();

        if (fallback is null)
        {
            return null;
        }

        return new AreaLocationMatch(
            fallback.Area.Id,
            fallback.Area.Name,
            fallback.Area.Code,
            fallback.Area.ParentAreaId!.Value,
            fallback.Area.DistrictName);
    }

    private static BoundaryProximity?
        TryGetBoundaryProximity(
            string geoJson,
            decimal latitude,
            decimal longitude)
    {
        if (string.IsNullOrWhiteSpace(geoJson))
        {
            return null;
        }

        try
        {
            using var document =
                JsonDocument.Parse(geoJson);

            var minimumLatitude =
                double.PositiveInfinity;

            var maximumLatitude =
                double.NegativeInfinity;

            var minimumLongitude =
                double.PositiveInfinity;

            var maximumLongitude =
                double.NegativeInfinity;

            var hasCoordinate = false;

            CollectCoordinates(
                document.RootElement,
                ref minimumLatitude,
                ref maximumLatitude,
                ref minimumLongitude,
                ref maximumLongitude,
                ref hasCoordinate);

            if (!hasCoordinate)
            {
                return null;
            }

            var pointLatitude =
                (double)latitude;

            var pointLongitude =
                (double)longitude;

            /*
             * Tìm điểm gần nhất trên bounding box.
             */
            var nearestLatitude =
                Math.Clamp(
                    pointLatitude,
                    minimumLatitude,
                    maximumLatitude);

            var nearestLongitude =
                Math.Clamp(
                    pointLongitude,
                    minimumLongitude,
                    maximumLongitude);

            var distanceMeters =
                CalculateDistanceMeters(
                    pointLatitude,
                    pointLongitude,
                    nearestLatitude,
                    nearestLongitude);

            var boundingBoxArea =
                Math.Abs(
                    (maximumLatitude - minimumLatitude)
                    * (maximumLongitude - minimumLongitude));

            return new BoundaryProximity(
                distanceMeters,
                boundingBoxArea);
        }
        catch (
            Exception exception)
            when (exception is JsonException
                or InvalidOperationException
                or FormatException
                or OverflowException)
        {
            return null;
        }
    }

    private static void CollectCoordinates(
        JsonElement element,
        ref double minimumLatitude,
        ref double maximumLatitude,
        ref double minimumLongitude,
        ref double maximumLongitude,
        ref bool hasCoordinate)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            /*
             * Geometry trực tiếp:
             * Polygon, MultiPolygon...
             */
            if (element.TryGetProperty(
                    "coordinates",
                    out var coordinates))
            {
                CollectCoordinates(
                    coordinates,
                    ref minimumLatitude,
                    ref maximumLatitude,
                    ref minimumLongitude,
                    ref maximumLongitude,
                    ref hasCoordinate);
            }

            /*
             * GeoJSON Feature.
             */
            if (element.TryGetProperty(
                    "geometry",
                    out var geometry))
            {
                CollectCoordinates(
                    geometry,
                    ref minimumLatitude,
                    ref maximumLatitude,
                    ref minimumLongitude,
                    ref maximumLongitude,
                    ref hasCoordinate);
            }

            /*
             * GeoJSON FeatureCollection.
             */
            if (element.TryGetProperty(
                    "features",
                    out var features))
            {
                CollectCoordinates(
                    features,
                    ref minimumLatitude,
                    ref maximumLatitude,
                    ref minimumLongitude,
                    ref maximumLongitude,
                    ref hasCoordinate);
            }

            return;
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        /*
         * Một điểm GeoJSON có dạng:
         * [longitude, latitude]
         */
        if (element.GetArrayLength() >= 2)
        {
            var enumerator =
                element.EnumerateArray();

            enumerator.MoveNext();
            var first = enumerator.Current;

            enumerator.MoveNext();
            var second = enumerator.Current;

            if (
                first.ValueKind == JsonValueKind.Number
                && second.ValueKind == JsonValueKind.Number)
            {
                var currentLongitude =
                    first.GetDouble();

                var currentLatitude =
                    second.GetDouble();

                if (
                    double.IsFinite(currentLongitude)
                    && double.IsFinite(currentLatitude)
                    && currentLongitude is >= -180 and <= 180
                    && currentLatitude is >= -90 and <= 90)
                {
                    minimumLatitude =
                        Math.Min(
                            minimumLatitude,
                            currentLatitude);

                    maximumLatitude =
                        Math.Max(
                            maximumLatitude,
                            currentLatitude);

                    minimumLongitude =
                        Math.Min(
                            minimumLongitude,
                            currentLongitude);

                    maximumLongitude =
                        Math.Max(
                            maximumLongitude,
                            currentLongitude);

                    hasCoordinate = true;
                }

                return;
            }
        }

        foreach (var child in element.EnumerateArray())
        {
            CollectCoordinates(
                child,
                ref minimumLatitude,
                ref maximumLatitude,
                ref minimumLongitude,
                ref maximumLongitude,
                ref hasCoordinate);
        }
    }

    private static double CalculateDistanceMeters(
        double firstLatitude,
        double firstLongitude,
        double secondLatitude,
        double secondLongitude)
    {
        var latitudeDifference =
            DegreesToRadians(
                secondLatitude - firstLatitude);

        var longitudeDifference =
            DegreesToRadians(
                secondLongitude - firstLongitude);

        var firstLatitudeRadians =
            DegreesToRadians(firstLatitude);

        var secondLatitudeRadians =
            DegreesToRadians(secondLatitude);

        var haversine =
            Math.Pow(
                Math.Sin(latitudeDifference / 2),
                2)
            + Math.Cos(firstLatitudeRadians)
            * Math.Cos(secondLatitudeRadians)
            * Math.Pow(
                Math.Sin(longitudeDifference / 2),
                2);

        var centralAngle =
            2 * Math.Atan2(
                Math.Sqrt(haversine),
                Math.Sqrt(
                    Math.Max(0, 1 - haversine)));

        return EarthRadiusMeters
            * centralAngle;
    }

    private static double DegreesToRadians(
        double degrees)
    {
        return degrees
            * Math.PI
            / 180d;
    }

    private readonly record struct BoundaryProximity(
        double DistanceMeters,
        double BoundingBoxArea);
}
