namespace UrbanIssue.API.Contracts.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    bool IsActive);
