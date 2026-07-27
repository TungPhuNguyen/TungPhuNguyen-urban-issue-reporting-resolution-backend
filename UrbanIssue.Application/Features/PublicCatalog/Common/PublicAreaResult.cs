namespace UrbanIssue.Application.Features.PublicCatalog.Common;

public sealed record PublicAreaResult(
    int Id,
    string Name,
    string? Code,
    int? ParentAreaId);
