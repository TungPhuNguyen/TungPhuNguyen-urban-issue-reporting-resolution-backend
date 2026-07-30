using MediatR;
using UrbanIssue.Application.Features.PublicCatalog.Common;

namespace UrbanIssue.Application.Features.PublicCatalog.GetPublicCategories;

public sealed record GetPublicCategoriesQuery
    : IRequest<IReadOnlyList<PublicCategoryResult>>;
