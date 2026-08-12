using MediatR;

namespace UrbanIssue.Application.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(
    int Id)
    : IRequest<bool>;
