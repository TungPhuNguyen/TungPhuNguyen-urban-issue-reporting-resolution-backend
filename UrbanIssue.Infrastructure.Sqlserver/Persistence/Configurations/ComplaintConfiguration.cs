using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations;

public sealed class ComplaintConfiguration
    : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("Complaints");
        builder.HasKey(complaint => complaint.Id);

        builder.Property(complaint => complaint.Reason)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(complaint => complaint.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ComplaintStatus.Pending)
            .IsRequired();

        builder.Property(complaint => complaint.AdminDecisionReason)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.HasIndex(complaint => complaint.ReportId)
            .IsUnique();

        builder.HasIndex(complaint => new
        {
            complaint.Status,
            complaint.CreatedAt
        });

        builder.HasOne(complaint => complaint.Report)
            .WithMany(report => report.Complaints)
            .HasForeignKey(complaint => complaint.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(complaint => complaint.Citizen)
            .WithMany(user => user.SubmittedComplaints)
            .HasForeignKey(complaint => complaint.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(complaint => complaint.ResolvedByAdmin)
            .WithMany(user => user.ResolvedComplaints)
            .HasForeignKey(complaint => complaint.ResolvedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
