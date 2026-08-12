namespace UrbanIssue.Domain.Entities
{
    public class ReportImage
    {
        public int Id { get; set; }

        public Guid ReportId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        // Navigation property
        public Report Report { get; set; } = null!;
    }
}
