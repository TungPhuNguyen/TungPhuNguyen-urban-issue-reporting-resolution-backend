using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasIndex(user => new
            {
                user.RoleId,
                user.DepartmentId,
                user.IsActive,
                user.CreatedAt
            });


            builder.Property(x => x.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.RefreshTokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.CreatedReports)
                .WithOne(x => x.Citizen)
                .HasForeignKey(x => x.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.AssignedReports)
                .WithOne(x => x.AssignedStaff)
                .HasForeignKey(x => x.AssignedStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.RejectedReports)
                .WithOne(x => x.RejectedByUser)
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ReopenedReports)
                .WithOne(x => x.ReopenedByUser)
                .HasForeignKey(x => x.ReopenedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.StatusUpdates)
                .WithOne(x => x.UpdatedByUser)
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Upvotes)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Comments)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Notifications)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.AuditLogs)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
