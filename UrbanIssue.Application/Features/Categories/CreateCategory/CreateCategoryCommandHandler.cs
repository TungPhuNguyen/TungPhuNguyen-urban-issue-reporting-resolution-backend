using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Application.Features.Categories.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Categories.CreateCategory
{
    public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryResult>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly DefaultSlaSettings _defaultSlaSettings;

        public CreateCategoryCommandHandler(
            IApplicationDbContext dbContext,
            IOptions<DefaultSlaSettings> defaultSlaOptions)
        {
            _dbContext = dbContext;
            _defaultSlaSettings = defaultSlaOptions.Value;
        }

        public async Task<CategoryResult> Handle(
            CreateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedName =
                request.Name.Trim();

            var normalizedDescription =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim();

            var categoryNameExists =
                await _dbContext.Categories
                    .AnyAsync(
                        category =>
                            category.Name == normalizedName,
                        cancellationToken);

            if (categoryNameExists)
            {
                throw new ConflictException(
                    $"Loại sự cố '{normalizedName}' đã tồn tại.");
            }

            var currentTime =
                DateTime.UtcNow;

            var category =
                new Category
                {
                    Name = normalizedName,
                    Description = normalizedDescription,
                    IsActive = true,
                    CreatedAt = currentTime,
                    UpdatedAt = null
                };

            var slaConfigs =
                new[]
                {
                new SLAConfig
                {
                    Category = category,
                    Priority = ReportPriority.Low,
                    DurationHours =
                        _defaultSlaSettings.LowHours,
                    CreatedAt = currentTime,
                    UpdatedAt = null
                },

                new SLAConfig
                {
                    Category = category,
                    Priority = ReportPriority.Medium,
                    DurationHours =
                        _defaultSlaSettings.MediumHours,
                    CreatedAt = currentTime,
                    UpdatedAt = null
                },

                new SLAConfig
                {
                    Category = category,
                    Priority = ReportPriority.High,
                    DurationHours =
                        _defaultSlaSettings.HighHours,
                    CreatedAt = currentTime,
                    UpdatedAt = null
                }
                };

            _dbContext.Categories.Add(
                category);

            _dbContext.SLAConfigs.AddRange(
                slaConfigs);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return new CategoryResult(
                Id: category.Id,
                Name: category.Name,
                Description: category.Description,
                IsOther: category.IsOther,
                IsActive: category.IsActive,
                CreatedAt: category.CreatedAt,
                UpdatedAt: category.UpdatedAt);
        }
    }
}
