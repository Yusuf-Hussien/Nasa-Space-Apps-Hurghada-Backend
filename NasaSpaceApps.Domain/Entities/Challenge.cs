namespace NasaSpaceApps.Domain.Entities;

public class Challenge
{
    public int ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Source { get; set; }
    public string? Season { get; set; }
    public string? Metadata { get; set; }
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
