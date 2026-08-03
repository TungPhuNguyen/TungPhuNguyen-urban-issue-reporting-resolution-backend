using System.Text.Json;

namespace UrbanIssue.Application.Common.Geography;

public static class GeoJsonBoundary
{
    private const double Epsilon = 1e-10;

    public static string NormalizeGeometry(string geoJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(geoJson);
        try
        {
            using var document = JsonDocument.Parse(geoJson);
            var geometry = GetGeometry(document.RootElement);
            _ = GetSupportedGeometryType(geometry);
            ValidateCoordinates(geometry);
            return geometry.GetRawText();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "GeoJSON không đúng định dạng JSON.",
                exception);
        }
    }

    public static bool Contains(
        string geoJson,
        decimal latitude,
        decimal longitude)
    {
        using var document = JsonDocument.Parse(geoJson);
        var geometry = GetGeometry(document.RootElement);
        var type = GetSupportedGeometryType(geometry);
        var coordinates = geometry.GetProperty("coordinates");
        var x = (double)longitude;
        var y = (double)latitude;

        return type switch
        {
            "Polygon" => PolygonContains(coordinates, x, y),
            "MultiPolygon" => coordinates
                .EnumerateArray()
                .Any(polygon => PolygonContains(polygon, x, y)),
            _ => false
        };
    }

    private static JsonElement GetGeometry(JsonElement root)
    {
        if (!root.TryGetProperty("type", out var typeElement))
        {
            throw new InvalidOperationException("GeoJSON thiếu thuộc tính type.");
        }

        var type = typeElement.GetString();
        if (string.Equals(type, "Feature", StringComparison.Ordinal))
        {
            if (!root.TryGetProperty("geometry", out var geometry)
                || geometry.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("GeoJSON Feature thiếu geometry.");
            }

            return geometry;
        }

        return root;
    }

    private static string GetSupportedGeometryType(JsonElement geometry)
    {
        if (!geometry.TryGetProperty("type", out var typeElement))
        {
            throw new InvalidOperationException("GeoJSON geometry thiếu thuộc tính type.");
        }

        var type = typeElement.GetString();
        if (type is not ("Polygon" or "MultiPolygon"))
        {
            throw new InvalidOperationException(
                "Ranh giới chỉ hỗ trợ GeoJSON Polygon hoặc MultiPolygon.");
        }

        return type;
    }

    private static void ValidateCoordinates(JsonElement geometry)
    {
        if (!geometry.TryGetProperty("coordinates", out var coordinates)
            || coordinates.ValueKind != JsonValueKind.Array
            || coordinates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("GeoJSON không có tọa độ hợp lệ.");
        }

        var type = geometry.GetProperty("type").GetString();
        var polygons = type == "Polygon"
            ? new[] { coordinates }
            : coordinates.EnumerateArray().ToArray();

        foreach (var polygon in polygons)
        {
            if (polygon.ValueKind != JsonValueKind.Array
                || polygon.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Polygon phải có ít nhất một vòng biên.");
            }

            foreach (var ring in polygon.EnumerateArray())
            {
                if (ring.ValueKind != JsonValueKind.Array
                    || ring.GetArrayLength() < 4)
                {
                    throw new InvalidOperationException(
                        "Mỗi vòng biên polygon phải có ít nhất 4 điểm.");
                }

                foreach (var position in ring.EnumerateArray())
                {
                    if (position.ValueKind != JsonValueKind.Array
                        || position.GetArrayLength() < 2
                        || position[0].ValueKind != JsonValueKind.Number
                        || position[1].ValueKind != JsonValueKind.Number)
                    {
                        throw new InvalidOperationException(
                            "Mỗi điểm GeoJSON phải có dạng [longitude, latitude].");
                    }
                }
            }
        }
    }

    private static bool PolygonContains(
        JsonElement polygon,
        double x,
        double y)
    {
        var rings = polygon.EnumerateArray().ToArray();
        if (rings.Length == 0 || !RingContains(rings[0], x, y))
        {
            return false;
        }

        return !rings.Skip(1).Any(hole => RingContains(hole, x, y));
    }

    private static bool RingContains(
        JsonElement ring,
        double x,
        double y)
    {
        var points = ring
            .EnumerateArray()
            .Select(position => (
                X: position[0].GetDouble(),
                Y: position[1].GetDouble()))
            .ToArray();
        var inside = false;

        for (var index = 0; index < points.Length; index++)
        {
            var current = points[index];
            var previous = points[(index + points.Length - 1) % points.Length];

            if (IsPointOnSegment(x, y, previous.X, previous.Y, current.X, current.Y))
            {
                return true;
            }

            var crossesRay = (current.Y > y) != (previous.Y > y)
                && x < (previous.X - current.X)
                    * (y - current.Y)
                    / (previous.Y - current.Y)
                    + current.X;
            if (crossesRay)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static bool IsPointOnSegment(
        double x,
        double y,
        double x1,
        double y1,
        double x2,
        double y2)
    {
        var cross = (y - y1) * (x2 - x1) - (x - x1) * (y2 - y1);
        if (Math.Abs(cross) > Epsilon)
        {
            return false;
        }

        return x >= Math.Min(x1, x2) - Epsilon
            && x <= Math.Max(x1, x2) + Epsilon
            && y >= Math.Min(y1, y2) - Epsilon
            && y <= Math.Max(y1, y2) + Epsilon;
    }
}
