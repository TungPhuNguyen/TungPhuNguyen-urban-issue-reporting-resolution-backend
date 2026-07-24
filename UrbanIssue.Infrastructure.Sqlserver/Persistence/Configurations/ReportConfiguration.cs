using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class ReportConfiguration
    : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Description)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.AddressText)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.Latitude)
                .HasPrecision(9, 6)
                .IsRequired();

            builder.Property(x => x.Longitude)
                .HasPrecision(9, 6)
                .IsRequired();

            builder.Property(x => x.Priority)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ReportStatus.New)
                .IsRequired();

            builder.Property(x => x.AppliedSLAHours)
                .IsRequired(false);

            builder.Property(x => x.RequiresManualAssignment)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.IsEscalated)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.RejectedReason)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.ReopenReason)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => new
            {
                x.Status,
                x.CreatedAt
            });

            builder.HasIndex(x => new
            {
                x.CategoryId,
                x.AreaId,
                x.Status
            });

            builder.HasIndex(x => new
            {
                x.Status,
                x.ComplaintSubmittedAt
            });

            builder.HasIndex(x => new
            {
                x.DepartmentId,
                x.Status,
                x.DueAt
            });

            builder.HasIndex(x => new
            {
                x.AssignedStaffId,
                x.Status
            });

            builder.HasIndex(x => new
            {
                x.RequiresManualAssignment,
                x.CreatedAt
            });

            builder.HasOne(x => x.Citizen)
                .WithMany(x => x.CreatedReports)
                .HasForeignKey(x => x.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Reports)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Area)
                .WithMany(x => x.Reports)
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany(x => x.Reports)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedStaff)
                .WithMany(x => x.AssignedReports)
                .HasForeignKey(x => x.AssignedStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SLAConfig)
                .WithMany(x => x.Reports)
                .HasForeignKey(x => x.SLAConfigId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RejectedByUser)
                .WithMany(x => x.RejectedReports)
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReopenedByUser)
                .WithMany(x => x.ReopenedReports)
                .HasForeignKey(x => x.ReopenedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.ComplaintReason)
                    .HasMaxLength(2000);
        }
    }
}
