using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Domain.Entities
{
    public class SLAConfig
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public ReportPriority Priority { get; set; }

        public int DurationHours { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Category Category { get; set; } = null!;

        public ICollection<Report> Reports { get; set; }
            = new List<Report>();
    }
}
