namespace UrbanIssue.Domain.Entities
{
    public class Upvote
    {
        public int Id { get; set; }

        public Guid ReportId { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Report Report { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
