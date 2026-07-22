using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Domain.Entities
{
    public class StatusUpdate
    {
        public int Id { get; set; }

        public Guid ReportId { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public ReportStatus OldStatus { get; set; }

        public ReportStatus NewStatus { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Report Report { get; set; } = null!;

        public User? UpdatedByUser { get; set; }

        public ICollection<StatusUpdateImage> Images { get; set; }
        = new List<StatusUpdateImage>();
    }
}
