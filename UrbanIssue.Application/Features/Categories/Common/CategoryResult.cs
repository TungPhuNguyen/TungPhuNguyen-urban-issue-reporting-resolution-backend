using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Categories.Common
{
    public sealed record CategoryResult(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
}
