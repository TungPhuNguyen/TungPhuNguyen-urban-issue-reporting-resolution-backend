using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Common.Interfaces.Geography;

public interface IAreaBoundaryService
{
    Task<AreaBoundaryCheckResult> CheckAsync(
        int areaId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken);

    Task<AreaLocationMatch?> FindContainingWardAsync(
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken);
}
