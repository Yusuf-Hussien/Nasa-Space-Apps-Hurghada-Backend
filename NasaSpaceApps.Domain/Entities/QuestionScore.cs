namespace NasaSpaceApps.Domain.Entities;

/// <summary>
/// A judge's score for one question of one criterion for one team.
/// Covers both catalogue questions (<see cref="CriterionQuestionId"/> set) and
/// ad-hoc "other" questions a judge adds on the fly
/// (<see cref="CriterionQuestionId"/> null, <see cref="QuestionName"/> optional).
/// When <see cref="Skipped"/> is true the entry is excluded from the result.
/// </summary>
public class QuestionScore
{
    public int QuestionScoreId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int CriterionId { get; set; }
    public JudgingCriterion? Criterion { get; set; }
    public int? CriterionQuestionId { get; set; }
    public CriterionQuestion? CriterionQuestion { get; set; }
    public string? QuestionName { get; set; }
    public int JudgeId { get; set; }
    public Judge? Judge { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Score { get; set; }
    public bool Skipped { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
