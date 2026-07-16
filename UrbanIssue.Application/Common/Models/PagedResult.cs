using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Common.Models
{
    public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalItems,
    int TotalPages);
}
