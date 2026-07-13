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
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.IsComplaint)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ReportId,
                x.CreatedAt
            });

            /*
             * Mỗi Report chỉ có tối đa một Comment
             * được đánh dấu là khiếu nại.
             */
            builder.HasIndex(x => x.ReportId)
                .IsUnique()
                .HasFilter("[IsComplaint] = 1");

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
