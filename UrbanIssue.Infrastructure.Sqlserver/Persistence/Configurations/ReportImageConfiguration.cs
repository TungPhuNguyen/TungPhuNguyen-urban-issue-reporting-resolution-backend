using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class ReportImageConfiguration
    : IEntityTypeConfiguration<ReportImage>
    {
        public void Configure(EntityTypeBuilder<ReportImage> builder)
        {
            builder.ToTable("ReportImages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.UploadedAt)
                .IsRequired();

            builder.HasIndex(x => x.ReportId);

            builder.HasOne(x => x.Report)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
