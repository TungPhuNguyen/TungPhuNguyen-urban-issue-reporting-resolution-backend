using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public Guid? ReportId { get; set; }

        public NotificationType Type { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;

        public Report? Report { get; set; }
    }
}
