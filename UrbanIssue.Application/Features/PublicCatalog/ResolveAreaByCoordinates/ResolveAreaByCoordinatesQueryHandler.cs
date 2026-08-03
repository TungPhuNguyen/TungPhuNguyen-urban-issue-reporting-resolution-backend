using MediatR;
using UrbanIssue.Application.Common.Interfaces.Geography;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Features.PublicCatalog.ResolveAreaByCoordinates;

public sealed class ResolveAreaByCoordinatesQueryHandler
    : IRequestHandler<ResolveAreaByCoordinatesQuery, AreaLocationMatch>
{
    private readonly IAreaBoundaryService _areaBoundaryService;

    public ResolveAreaByCoordinatesQueryHandler(
        IAreaBoundaryService areaBoundaryService)
    {
        _areaBoundaryService = areaBoundaryService;
    }

    public async Task<AreaLocationMatch> Handle(
        ResolveAreaByCoordinatesQuery request,
        CancellationToken cancellationToken)
    {
        return await _areaBoundaryService.FindContainingWardAsync(
                request.Latitude,
                request.Longitude,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy phường/xã có polygon chứa tọa độ đã chọn.");
    }
}
