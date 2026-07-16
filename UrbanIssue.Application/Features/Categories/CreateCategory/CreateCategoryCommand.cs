using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Application.Features.Categories.Common;

namespace UrbanIssue.Application.Features.Categories.CreateCategory
{
    public sealed record CreateCategoryCommand(
    string Name,
    string? Description)
    : IRequest<CategoryResult>;
}
