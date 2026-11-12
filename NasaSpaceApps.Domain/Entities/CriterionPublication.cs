namespace NasaSpaceApps.Domain.Entities;

/// <summary>
/// Marks that a judge has published their score for a team on a criterion,
/// making that score visible on the live scoreboard. Visibility is per
/// (judge, team, criterion): every judge can submit and publish a team's
/// criterion independently, and the board averages the judges who have published.
/// </summary>
public class CriterionPublication
{
    public int CriterionPublicationId { get; set; }
    public int CriterionId { get; set; }
    public JudgingCriterion? Criterion { get; set; }
    public int JudgeId { get; set; }
    public Judge? Judge { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}
