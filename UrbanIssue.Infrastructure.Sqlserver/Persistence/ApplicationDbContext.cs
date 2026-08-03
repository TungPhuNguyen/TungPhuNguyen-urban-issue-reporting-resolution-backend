using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Application.Common.Interfaces.Notifications;

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence;

public sealed class ApplicationDbContext
    : DbContext,
      IApplicationDbContext
{
    private readonly INotificationRealtimePublisher? _notificationPublisher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        INotificationRealtimePublisher? notificationPublisher = null)
        : base(options)
    {
        _notificationPublisher = notificationPublisher;
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

    public DbSet<Complaint> Complaints
        => Set<Complaint>();

    public DbSet<ComplaintImage> ComplaintImages
        => Set<ComplaintImage>();

    public DbSet<AuditLog> AuditLogs
        => Set<AuditLog>();

    protected override void OnModelCreating(
    ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("ReportNumbers")
            .StartsAt(100001)
            .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var pendingNotifications = ChangeTracker
            .Entries<Notification>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_notificationPublisher is not null
            && pendingNotifications.Count > 0)
        {
            await _notificationPublisher.PublishAsync(
                pendingNotifications
                    .Select(notification => new NotificationRealtimeMessage(
                        notification.Id,
                        notification.UserId,
                        notification.ReportId,
                        notification.Type.ToString(),
                        notification.Title,
                        notification.Message,
                        notification.CreatedAt))
                    .ToList(),
                cancellationToken);
        }

        return result;
    }
}
