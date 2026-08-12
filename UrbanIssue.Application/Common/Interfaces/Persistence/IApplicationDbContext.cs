using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Common.Interfaces.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<Role> Roles { get; }

        DbSet<User> Users { get; }

        DbSet<RefreshToken> RefreshTokens { get; }

        DbSet<Category> Categories { get; }

        DbSet<Area> Areas { get; }

        DbSet<Department> Departments { get; }

        DbSet<RoutingRule> RoutingRules { get; }

        DbSet<SLAConfig> SLAConfigs { get; }

        DbSet<Report> Reports { get; }

        DbSet<ReportImage> ReportImages { get; }

        DbSet<StatusUpdate> StatusUpdates { get; }

        DbSet<StatusUpdateImage> StatusUpdateImages { get; }

        DbSet<Upvote> Upvotes { get; }

        DbSet<Comment> Comments { get; }

        DbSet<Notification> Notifications { get; }

        DbSet<Complaint> Complaints { get; }

        DbSet<ComplaintImage> ComplaintImages { get; }

        DbSet<AuditLog> AuditLogs { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
