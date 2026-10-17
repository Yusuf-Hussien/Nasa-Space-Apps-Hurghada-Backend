namespace NasaSpaceApps.Domain.Entities;

public class ServiceRequest
{
    public int RequestId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
