using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.RoutingRules.Common;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Features.RoutingRules.CreateRoutingRule;

public sealed class CreateRoutingRuleCommandHandler
    : IRequestHandler<
        CreateRoutingRuleCommand,
        RoutingRuleResult>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateRoutingRuleCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoutingRuleResult> Handle(
        CreateRoutingRuleCommand request,
        CancellationToken cancellationToken)
    {
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

        if (!category.IsActive)
        {
            throw new ConflictException(
                "Không thể sử dụng loại sự cố đã ngừng hoạt động.");
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

        if (!area.IsActive)
        {
            throw new ConflictException(
                "Không thể sử dụng khu vực đã ngừng hoạt động.");
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

        if (!department.IsActive)
        {
            throw new ConflictException(
                "Không thể sử dụng đơn vị xử lý đã ngừng hoạt động.");
        }

        var duplicateExists =
            await _dbContext.RoutingRules
                .AnyAsync(
                    rule =>
                        rule.CategoryId == request.CategoryId
                        && rule.AreaId == request.AreaId
                        && rule.DepartmentId
                            == request.DepartmentId,
                    cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                "Quy tắc định tuyến với loại sự cố, khu vực "
                + "và đơn vị xử lý này đã tồn tại.");
        }

        var currentTime =
            DateTime.UtcNow;

        var routingRule =
            new RoutingRule
            {
                CategoryId = request.CategoryId,
                AreaId = request.AreaId,
                DepartmentId = request.DepartmentId,
                PriorityOrder = request.PriorityOrder,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = null
            };

        _dbContext.RoutingRules.Add(
            routingRule);

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
