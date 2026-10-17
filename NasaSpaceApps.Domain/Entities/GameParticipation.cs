namespace NasaSpaceApps.Domain.Entities;

public class GameParticipation
{
    public int ParticipationId { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string? Answer { get; set; }
    public bool Correct { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
