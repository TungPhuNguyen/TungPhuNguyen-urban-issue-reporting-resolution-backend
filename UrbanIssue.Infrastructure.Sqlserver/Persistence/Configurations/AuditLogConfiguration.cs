using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class AuditLogConfiguration
    : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.EntityType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.EntityId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Detail)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.EntityType,
                x.EntityId
            });

            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.User)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
