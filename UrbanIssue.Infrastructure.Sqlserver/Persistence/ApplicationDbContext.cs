using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence;

public sealed class ApplicationDbContext
    : DbContext,
      IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles
        => Set<Role>();

    public DbSet<User> Users
        => Set<User>();

    public DbSet<RefreshToken> RefreshTokens
        => Set<RefreshToken>();

    public DbSet<Category> Categories
        => Set<Category>();

    public DbSet<Area> Areas
        => Set<Area>();

    public DbSet<Department> Departments
        => Set<Department>();

    public DbSet<RoutingRule> RoutingRules
        => Set<RoutingRule>();

    public DbSet<SLAConfig> SLAConfigs
        => Set<SLAConfig>();

    public DbSet<Report> Reports
        => Set<Report>();

    public DbSet<ReportImage> ReportImages
        => Set<ReportImage>();

    public DbSet<StatusUpdate> StatusUpdates
        => Set<StatusUpdate>();

    public DbSet<StatusUpdateImage> StatusUpdateImages
        => Set<StatusUpdateImage>();

    public DbSet<Upvote> Upvotes
        => Set<Upvote>();

    public DbSet<Comment> Comments
        => Set<Comment>();

    public DbSet<Notification> Notifications
        => Set<Notification>();

    public DbSet<AuditLog> AuditLogs
        => Set<AuditLog>();

    protected override void OnModelCreating(
    ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
