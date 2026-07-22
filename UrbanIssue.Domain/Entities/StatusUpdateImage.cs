namespace UrbanIssue.Domain.Entities;

public class StatusUpdateImage
{
    public int Id { get; set; }

    public int StatusUpdateId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public StatusUpdate StatusUpdate { get; set; } = null!;
}
