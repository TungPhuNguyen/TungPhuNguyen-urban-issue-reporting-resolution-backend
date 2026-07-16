using MediatR;
using UrbanIssue.Application.Features.Categories.Common;

namespace UrbanIssue.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    int Id,
    string Name,
    string? Description,
    bool IsActive)
    : IRequest<CategoryResult>;
