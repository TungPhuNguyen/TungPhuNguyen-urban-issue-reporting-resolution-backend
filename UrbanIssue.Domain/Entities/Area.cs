using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Domain.Entities
{
    public class Area
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? ParentAreaId { get; set; }

        public string? Code { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Self-reference navigation
        public Area? ParentArea { get; set; }

        public ICollection<Area> ChildAreas { get; set; }
            = new List<Area>();

        // Navigation properties
        public ICollection<RoutingRule> RoutingRules { get; set; }
            = new List<RoutingRule>();

        public ICollection<Report> Reports { get; set; }
            = new List<Report>();
    }
}
