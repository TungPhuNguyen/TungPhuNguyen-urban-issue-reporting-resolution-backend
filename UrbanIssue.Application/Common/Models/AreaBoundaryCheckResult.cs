namespace UrbanIssue.Application.Common.Models;

public sealed record AreaBoundaryCheckResult(
    bool HasBoundary,
    bool ContainsPoint);

public sealed record AreaLocationMatch(
    int AreaId,
    string AreaName,
    string? AreaCode,
    int DistrictId,
    string DistrictName);
