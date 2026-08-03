using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations;

public sealed class ComplaintImageConfiguration
    : IEntityTypeConfiguration<ComplaintImage>
{
    public void Configure(EntityTypeBuilder<ComplaintImage> builder)
    {
        builder.ToTable("ComplaintImages");
        builder.HasKey(image => image.Id);

        builder.Property(image => image.ImageUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(image => image.ComplaintId);

        builder.HasOne(image => image.Complaint)
            .WithMany(complaint => complaint.Images)
            .HasForeignKey(image => image.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
