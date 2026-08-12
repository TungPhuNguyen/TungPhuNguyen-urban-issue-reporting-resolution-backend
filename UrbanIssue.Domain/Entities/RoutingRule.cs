namespace UrbanIssue.Domain.Entities
{
    public class RoutingRule
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public int AreaId { get; set; }

        public int DepartmentId { get; set; }

        public int PriorityOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Category Category { get; set; } = null!;

        public Area Area { get; set; } = null!;

        public Department Department { get; set; } = null!;
    }
}
