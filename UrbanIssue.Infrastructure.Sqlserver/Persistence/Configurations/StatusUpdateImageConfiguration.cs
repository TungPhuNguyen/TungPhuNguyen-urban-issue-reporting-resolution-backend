using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class StatusUpdateImageConfiguration
    : IEntityTypeConfiguration<StatusUpdateImage>
    {
        public void Configure(
            EntityTypeBuilder<StatusUpdateImage> builder)
        {
            builder.ToTable("StatusUpdateImages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(1000)
                .IsRequired();

            builder.HasIndex(x => x.StatusUpdateId);

            builder.HasOne(x => x.StatusUpdate)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.StatusUpdateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
