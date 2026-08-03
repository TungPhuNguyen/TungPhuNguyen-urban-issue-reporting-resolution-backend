using UrbanIssue.Application.Common.Geography;

namespace UrbanIssue.Application.Tests.Geography;

public sealed class GeoJsonBoundaryTests
{
    private const string Polygon =
        """
        {
          "type": "Polygon",
          "coordinates": [
            [[105.80, 21.00], [105.90, 21.00], [105.90, 21.10], [105.80, 21.10], [105.80, 21.00]],
            [[105.84, 21.04], [105.86, 21.04], [105.86, 21.06], [105.84, 21.06], [105.84, 21.04]]
          ]
        }
        """;

    [Fact]
    public void Contains_ReturnsTrue_ForPointInsideExteriorRing()
    {
        Assert.True(GeoJsonBoundary.Contains(Polygon, 21.02m, 105.82m));
    }

    [Fact]
    public void Contains_ReturnsFalse_ForPointInsideHole()
    {
        Assert.False(GeoJsonBoundary.Contains(Polygon, 21.05m, 105.85m));
    }

    [Fact]
    public void Contains_ReturnsTrue_ForPointOnBoundary()
    {
        Assert.True(GeoJsonBoundary.Contains(Polygon, 21.00m, 105.85m));
    }

    [Fact]
    public void NormalizeGeometry_ExtractsGeometryFromFeature()
    {
        var feature = $$"""
            {
              "type": "Feature",
              "properties": { "code": "HN-W-TEST" },
              "geometry": {{Polygon}}
            }
            """;

        var normalized = GeoJsonBoundary.NormalizeGeometry(feature);

        Assert.Contains("\"type\": \"Polygon\"", normalized);
        Assert.DoesNotContain("properties", normalized);
    }
}
