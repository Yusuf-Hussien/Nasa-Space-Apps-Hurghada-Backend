namespace NasaSpaceApps.Domain.Entities;

public class JudgingCriterion
{
    public int CriterionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
}
