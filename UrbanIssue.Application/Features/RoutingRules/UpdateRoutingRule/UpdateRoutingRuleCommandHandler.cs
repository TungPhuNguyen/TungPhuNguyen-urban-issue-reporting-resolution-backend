using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.RoutingRules.Common;

namespace UrbanIssue.Application.Features.RoutingRules.UpdateRoutingRule;

public sealed class UpdateRoutingRuleCommandHandler
    : IRequestHandler<
        UpdateRoutingRuleCommand,
        RoutingRuleResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateRoutingRuleCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoutingRuleResult> Handle(
        UpdateRoutingRuleCommand request,
        CancellationToken cancellationToken)
    {
        var routingRule =
            await _dbContext.RoutingRules
                .SingleOrDefaultAsync(
                    rule =>
                        rule.Id == request.Id,
                    cancellationToken);

        if (routingRule is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy quy tắc định tuyến có ID {request.Id}.");
        }

        var category =
            await _dbContext.Categories
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    category =>
                        category.Id == request.CategoryId,
                    cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại sự cố có ID {request.CategoryId}.");
        }

        var area =
            await _dbContext.Areas
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    area =>
                        area.Id == request.AreaId,
                    cancellationToken);

        if (area is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy khu vực có ID {request.AreaId}.");
        }

        var department =
            await _dbContext.Departments
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    department =>
                        department.Id == request.DepartmentId,
                    cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy đơn vị xử lý có ID {request.DepartmentId}.");
        }

        /*
         * Khi bật rule, các dữ liệu tham chiếu
         * bắt buộc phải đang hoạt động.
         */
        if (request.IsActive)
        {
            if (!category.IsActive)
            {
                throw new ConflictException(
                    "Không thể kích hoạt quy tắc sử dụng loại sự cố đã ngừng hoạt động.");
            }

            if (!area.IsActive)
            {
                throw new ConflictException(
                    "Không thể kích hoạt quy tắc sử dụng khu vực đã ngừng hoạt động.");
            }

            if (!department.IsActive)
            {
                throw new ConflictException(
                    "Không thể kích hoạt quy tắc sử dụng đơn vị xử lý đã ngừng hoạt động.");
            }
        }

        var duplicateExists =
            await _dbContext.RoutingRules
                .AnyAsync(
                    otherRule =>
                        otherRule.Id != request.Id
                        && otherRule.CategoryId
                            == request.CategoryId
                        && otherRule.AreaId
                            == request.AreaId
                        && otherRule.DepartmentId
                            == request.DepartmentId,
                    cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                "Quy tắc định tuyến với loại sự cố, khu vực "
                + "và đơn vị xử lý này đã tồn tại.");
        }

        routingRule.CategoryId =
            request.CategoryId;

        routingRule.AreaId =
            request.AreaId;

        routingRule.DepartmentId =
            request.DepartmentId;

        routingRule.PriorityOrder =
            request.PriorityOrder;

        routingRule.IsActive =
            request.IsActive;

        routingRule.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new RoutingRuleResult(
            Id: routingRule.Id,
            CategoryId: routingRule.CategoryId,
            CategoryName: category.Name,
            AreaId: routingRule.AreaId,
            AreaName: area.Name,
            DepartmentId: routingRule.DepartmentId,
            DepartmentName: department.Name,
            PriorityOrder: routingRule.PriorityOrder,
            IsActive: routingRule.IsActive,
            CreatedAt: routingRule.CreatedAt,
            UpdatedAt: routingRule.UpdatedAt);
    }
}
