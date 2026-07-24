using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class CommentConfiguration
    : IEntityTypeConfiguration<Comment>
    {
        public void Configure(
            EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ReportId,
                x.CreatedAt
            });

            builder.HasOne(x => x.Report)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
