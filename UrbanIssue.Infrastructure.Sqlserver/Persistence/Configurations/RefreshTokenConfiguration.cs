using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.RevokedAt)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => x.UserId);

            builder.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
