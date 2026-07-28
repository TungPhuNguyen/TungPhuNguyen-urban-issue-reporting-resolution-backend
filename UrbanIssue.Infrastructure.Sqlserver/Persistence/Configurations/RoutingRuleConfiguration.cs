using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations;

public sealed class RoutingRuleConfiguration
    : IEntityTypeConfiguration<RoutingRule>
{
    public void Configure(
        EntityTypeBuilder<RoutingRule> builder)
    {
        builder.ToTable("RoutingRules");

        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.PriorityOrder)
            .IsRequired();

        builder.Property(rule => rule.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(rule => rule.CreatedAt)
            .IsRequired();

        builder.Property(rule => rule.UpdatedAt)
            .IsRequired(false);

        /*
         * Mỗi Department chỉ có một RoutingRule cho cùng
         * Category + Area. PriorityOrder không thuộc unique key
         * vì nhiều Department có thể dùng cùng mức ưu tiên.
         */
        builder.HasIndex(rule => new
        {
            rule.CategoryId,
            rule.AreaId,
            rule.DepartmentId
        })
        .IsUnique();

        builder.HasOne(rule => rule.Category)
            .WithMany(category => category.RoutingRules)
            .HasForeignKey(rule => rule.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rule => rule.Area)
            .WithMany(area => area.RoutingRules)
            .HasForeignKey(rule => rule.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rule => rule.Department)
            .WithMany(department => department.RoutingRules)
            .HasForeignKey(rule => rule.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
