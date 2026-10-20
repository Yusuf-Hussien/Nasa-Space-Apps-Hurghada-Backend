namespace NasaSpaceApps.Domain.Entities;

public class Team
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    public string? Logo { get; set; }
    public string Status { get; set; } = "Active";
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public Project? Project { get; set; }
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
    public ICollection<ServiceRequest> Requests { get; set; } = new List<ServiceRequest>();
    public ICollection<GameParticipation> GameParticipations { get; set; } = new List<GameParticipation>();
}
