using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Constants;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Configurations
{
    public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasMany(x => x.Users)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            SeedRoles(builder);
        }

        private static void SeedRoles(
            EntityTypeBuilder<Role> builder)
        {
            var seedCreatedAt = new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            builder.HasData(
                new Role
                {
                    Id = 1,
                    Name = RoleNames.Citizen,
                    CreatedAt = seedCreatedAt,
                    UpdatedAt = null
                },
                new Role
                {
                    Id = 2,
                    Name = RoleNames.Staff,
                    CreatedAt = seedCreatedAt,
                    UpdatedAt = null
                },
                new Role
                {
                    Id = 3,
                    Name = RoleNames.Admin,
                    CreatedAt = seedCreatedAt,
                    UpdatedAt = null
                });
        }
    }
}
