using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Domain.Entities
{
    public class Report
    {
        public Guid Id { get; set; }

        // Foreign keys
        public Guid CitizenId { get; set; }

        public int CategoryId { get; set; }

        public int AreaId { get; set; }

        public int? DepartmentId { get; set; }

        public Guid? AssignedStaffId { get; set; }

        public int? SLAConfigId { get; set; }

        // Report information
        public string Description { get; set; } = string.Empty;

        public string? AddressText { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        // Status and priority
        public ReportPriority? Priority { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.New;

        // SLA snapshot
        public DateTime? SLAStartedAt { get; set; }

        public int? AppliedSLAHours { get; set; }

        public DateTime? DueAt { get; set; }

        // SLA notification and escalation
        public DateTime? SLAWarningSentAt { get; set; }

        public DateTime? SLABreachedNotifiedAt { get; set; }

        public bool IsEscalated { get; set; }

        public DateTime? EscalatedAt { get; set; }

        // Routing
        public bool RequiresManualAssignment { get; set; }

        // Tracking time
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        // Rejection
        public DateTime? RejectedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public string? RejectedReason { get; set; }

        // Reopen after complaint
        public DateTime? ReopenedAt { get; set; }

        public Guid? ReopenedByUserId { get; set; }

        public string? ReopenReason { get; set; }

        // Navigation properties
        public User Citizen { get; set; } = null!;

        public Category Category { get; set; } = null!;

        public Area Area { get; set; } = null!;

        public Department? Department { get; set; }

        public User? AssignedStaff { get; set; }

        public SLAConfig? SLAConfig { get; set; }

        public User? RejectedByUser { get; set; }

        public User? ReopenedByUser { get; set; }

        public ICollection<ReportImage> Images { get; set; }
            = new List<ReportImage>();

        public ICollection<StatusUpdate> StatusUpdates { get; set; }
            = new List<StatusUpdate>();

        public ICollection<Upvote> Upvotes { get; set; }
            = new List<Upvote>();

        public ICollection<Comment> Comments { get; set; }
            = new List<Comment>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();
    }
}
