namespace NasaSpaceApps.Domain.Entities;

public class Question
{
    public int QuestionId { get; set; }
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Options { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int Points { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public ICollection<GameParticipation> Participations { get; set; } = new List<GameParticipation>();
}
