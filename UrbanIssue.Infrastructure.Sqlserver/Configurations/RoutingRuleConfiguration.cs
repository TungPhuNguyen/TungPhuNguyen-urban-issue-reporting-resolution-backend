using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class RoutingRuleConfiguration
    : IEntityTypeConfiguration<RoutingRule>
    {
        public void Configure(EntityTypeBuilder<RoutingRule> builder)
        {
            builder.ToTable("RoutingRules");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PriorityOrder)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => new
            {
                x.CategoryId,
                x.AreaId,
                x.DepartmentId
            })
            .IsUnique();

            builder.HasIndex(x => new
            {
                x.CategoryId,
                x.AreaId,
                x.IsActive
            });

            builder.HasOne(x => x.Category)
                .WithMany(x => x.RoutingRules)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Area)
                .WithMany(x => x.RoutingRules)
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany(x => x.RoutingRules)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
