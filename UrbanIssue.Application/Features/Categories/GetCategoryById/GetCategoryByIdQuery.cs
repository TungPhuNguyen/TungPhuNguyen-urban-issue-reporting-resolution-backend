using MediatR;
using UrbanIssue.Application.Features.Categories.Common;

namespace UrbanIssue.Application.Features.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(
    int Id)
    : IRequest<CategoryResult>;
