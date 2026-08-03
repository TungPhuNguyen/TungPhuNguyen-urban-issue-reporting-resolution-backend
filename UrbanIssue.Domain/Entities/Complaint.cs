using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Domain.Entities;

public sealed class Complaint
{
    public int Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid CitizenId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
    public string? AdminDecisionReason { get; set; }
    public Guid? ResolvedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Report Report { get; set; } = null!;
    public User Citizen { get; set; } = null!;
    public User? ResolvedByAdmin { get; set; }
    public ICollection<ComplaintImage> Images { get; set; } =
        new List<ComplaintImage>();
}
