using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class UpvoteConfiguration
    : IEntityTypeConfiguration<Upvote>
    {
        public void Configure(EntityTypeBuilder<Upvote> builder)
        {
            builder.ToTable("Upvotes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.ReportId,
                x.UserId
            })
            .IsUnique();

            builder.HasOne(x => x.Report)
                .WithMany(x => x.Upvotes)
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Upvotes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
