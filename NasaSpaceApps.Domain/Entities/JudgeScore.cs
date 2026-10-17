namespace NasaSpaceApps.Domain.Entities;

public class JudgeScore
{
    public int ScoreId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int JudgeId { get; set; }
    public Judge? Judge { get; set; }
    public int CriterionId { get; set; }
    public JudgingCriterion? Criterion { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
