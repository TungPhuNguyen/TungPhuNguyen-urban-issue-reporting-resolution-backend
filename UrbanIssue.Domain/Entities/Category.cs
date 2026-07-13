using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<RoutingRule> RoutingRules { get; set; }
            = new List<RoutingRule>();

        public ICollection<SLAConfig> SLAConfigs { get; set; }
            = new List<SLAConfig>();

        public ICollection<Report> Reports { get; set; }
            = new List<Report>();
    }
}
