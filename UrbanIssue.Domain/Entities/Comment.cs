namespace UrbanIssue.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public Guid ReportId { get; set; }

        public Guid UserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsComplaint { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Report Report { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
