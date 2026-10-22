namespace NasaSpaceApps.Domain.Entities;

public class JudgingCriterion
{
    public int CriterionId { get; set; }
    public int? ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public ICollection<CriterionQuestion> Questions { get; set; } = new List<CriterionQuestion>();
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
    public ICollection<CriterionPublication> Publications { get; set; } = new List<CriterionPublication>();
}
