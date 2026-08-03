namespace UrbanIssue.Domain.Entities;

public sealed class ComplaintImage
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public Complaint Complaint { get; set; } = null!;
}
