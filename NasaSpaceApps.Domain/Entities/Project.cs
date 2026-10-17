namespace NasaSpaceApps.Domain.Entities;

public class Project
{
    public int ProjectId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RepositoryLink { get; set; }
    public string? DemoLink { get; set; }
}
