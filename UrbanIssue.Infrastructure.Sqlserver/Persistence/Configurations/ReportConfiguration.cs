using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations;

public sealed class ReportConfiguration
    : IEntityTypeConfiguration<Report>
{
    public void Configure(
        EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(report => report.Id);

        builder.Property(report => report.ReportNumber)
            .HasDefaultValueSql("NEXT VALUE FOR [ReportNumbers]")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.HasIndex(report => report.ReportNumber)
            .IsUnique();

        builder.Property(report => report.ReportCode)
            .HasMaxLength(30)
            .HasComputedColumnSql(
                "('UI-' + CONVERT([varchar](20),[ReportNumber]))",
                stored: true)
            .IsRequired(false);

        builder.HasIndex(report => report.ReportCode)
            .IsUnique()
            .HasFilter(null);

        builder.Property(report => report.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(report => report.Description)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(report => report.OtherCategoryText)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(report => report.AddressText)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(report => report.Latitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(report => report.Longitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(report => report.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(report => report.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReportStatus.New)
            .IsRequired();

        builder.Property(report => report.AppliedSLAHours)
            .IsRequired(false);

        builder.Property(
                report => report.RequiresManualAssignment)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(report => report.IsEscalated)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(report => report.HasSubmittedComplaint)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(report => report.RejectedReason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(report => report.ReopenReason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(report => report.ComplaintReason)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(report => report.CreatedAt)
            .IsRequired();

        builder.Property(report => report.RowVersion)
            .IsRowVersion();

        builder.Property(report => report.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(report => new
        {
            report.Status,
            report.CreatedAt
        });

        builder.HasIndex(report => new
        {
            report.CategoryId,
            report.AreaId,
            report.Status
        });

        builder.HasIndex(report => new
        {
            report.Status,
            report.ComplaintSubmittedAt
        });

        builder.HasIndex(report => new
        {
            report.HasSubmittedComplaint,
            report.Status
        });

        builder.HasIndex(report => new
        {
            report.DepartmentId,
            report.Status,
            report.DueAt
        });

        builder.HasIndex(report => new
        {
            report.AssignedStaffId,
            report.Status
        });

        builder.HasIndex(report => new
        {
            report.RequiresManualAssignment,
            report.CreatedAt
        });

        builder.HasIndex(report => new
        {
            report.Status,
            report.Latitude,
            report.Longitude
        });

        builder.HasIndex(report => new
        {
            report.CategoryId,
            report.AreaId,
            report.Status,
            report.CreatedAt
        });

        builder.HasIndex(report => report.CreatedAt);
        builder.HasIndex(report => report.ResolvedAt);
        builder.HasIndex(report => report.ClosedAt);

        builder.HasIndex(report => new
        {
            report.Status,
            report.DueAt
        });

        builder.HasIndex(report => report.SLAStartedAt);

        builder.HasOne(report => report.Citizen)
            .WithMany(user => user.CreatedReports)
            .HasForeignKey(report => report.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(report => report.Category)
            .WithMany(category => category.Reports)
            .HasForeignKey(report => report.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(report => report.Area)
            .WithMany(area => area.Reports)
            .HasForeignKey(report => report.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(report => report.Department)
            .WithMany(department => department.Reports)
            .HasForeignKey(report => report.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(report => report.AssignedStaff)
            .WithMany(user => user.AssignedReports)
            .HasForeignKey(report => report.AssignedStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(report => report.SLAConfig)
            .WithMany(config => config.Reports)
            .HasForeignKey(report => report.SLAConfigId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(report => report.RejectedByUser)
            .WithMany(user => user.RejectedReports)
            .HasForeignKey(report => report.RejectedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(report => report.ReopenedByUser)
            .WithMany(user => user.ReopenedReports)
            .HasForeignKey(report => report.ReopenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
