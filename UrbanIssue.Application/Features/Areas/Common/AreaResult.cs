namespace UrbanIssue.Application.Features.Areas.Common;

public sealed record AreaResult(
    int Id,
    string Name,
    string? Code,
    int? ParentAreaId,
    string? ParentAreaName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
