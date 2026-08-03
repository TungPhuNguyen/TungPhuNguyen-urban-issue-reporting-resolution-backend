using System.Xml.Linq;

namespace UrbanIssue.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public int RoleId { get; set; }

        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Role Role { get; set; } = null!;

        public Department? Department { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();

        public ICollection<Report> CreatedReports { get; set; }
            = new List<Report>();

        public ICollection<Report> AssignedReports { get; set; }
            = new List<Report>();

        public ICollection<Report> RejectedReports { get; set; }
            = new List<Report>();

        public ICollection<Report> ReopenedReports { get; set; }
            = new List<Report>();

        public ICollection<StatusUpdate> StatusUpdates { get; set; }
            = new List<StatusUpdate>();

        public ICollection<Upvote> Upvotes { get; set; }
            = new List<Upvote>();

        public ICollection<Comment> Comments { get; set; }
            = new List<Comment>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();

        public ICollection<AuditLog> AuditLogs { get; set; }
            = new List<AuditLog>();

        public ICollection<Complaint> SubmittedComplaints { get; set; }
            = new List<Complaint>();

        public ICollection<Complaint> ResolvedComplaints { get; set; }
            = new List<Complaint>();
    }
}
