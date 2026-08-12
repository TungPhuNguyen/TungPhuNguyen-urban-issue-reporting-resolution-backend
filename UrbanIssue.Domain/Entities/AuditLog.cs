namespace UrbanIssue.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public Guid? UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public string EntityId { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
