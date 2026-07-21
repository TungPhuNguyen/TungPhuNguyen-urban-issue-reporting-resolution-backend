namespace UrbanIssue.Application.Features.PublicCatalog.Common;

public sealed record PublicCategoryResult(
    int Id,
    string Name,
    string? Description);
