namespace NasaSpaceApps.Domain.Entities;

/// <summary>
/// An article/reference question shown to judges while scoring a criterion.
/// It carries no answer and is never auto-graded: it is a prompt the judge
/// reads before awarding a score.
/// </summary>
public class CriterionQuestion
{
    public int CriterionQuestionId { get; set; }
    public int CriterionId { get; set; }
    public JudgingCriterion? Criterion { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
