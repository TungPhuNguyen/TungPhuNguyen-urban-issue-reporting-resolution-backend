using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.Message)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.IsRead)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.ReadAt)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.UserId,
                x.IsRead,
                x.CreatedAt
            });

            builder.HasIndex(x => x.ReportId);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Report)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
