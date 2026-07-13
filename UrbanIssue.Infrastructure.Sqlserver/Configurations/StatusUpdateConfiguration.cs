using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class StatusUpdateConfiguration
    : IEntityTypeConfiguration<StatusUpdate>
    {
        public void Configure(EntityTypeBuilder<StatusUpdate> builder)
        {
            builder.ToTable("StatusUpdates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OldStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Note)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ReportId,
                x.CreatedAt
            });

            builder.HasOne(x => x.Report)
                .WithMany(x => x.StatusUpdates)
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UpdatedByUser)
                .WithMany(x => x.StatusUpdates)
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
