namespace UrbanIssue.Application.Features.Departments.Common;

public sealed record DepartmentResult(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
