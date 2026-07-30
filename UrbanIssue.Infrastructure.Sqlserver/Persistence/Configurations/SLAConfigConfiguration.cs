using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class SLAConfigConfiguration
    : IEntityTypeConfiguration<SLAConfig>
    {
        public void Configure(EntityTypeBuilder<SLAConfig> builder)
        {
            builder.ToTable("SLAConfigs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Priority)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.DurationHours)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => new { x.CategoryId, x.Priority }).IsUnique();

            builder.HasOne(x => x.Category)
                .WithMany(x => x.SLAConfigs)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
