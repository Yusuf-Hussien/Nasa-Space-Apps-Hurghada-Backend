using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// One row of the public scoreboard. Ranking is by <see cref="Percentage"/>
/// (achieved / available across criteria published by judges), not raw score.
/// Scores are averaged across the judges who published each criterion. Only
/// published totals are exposed — never the internal per-question scores.
/// </summary>
public sealed record StandingsEntryResponse(
    int? Rank,
    bool IsTied,
    int TeamId,
    string TeamName,
    decimal AchievedScore,
    decimal AvailableScore,
    decimal Percentage,
    int JudgesCounted,
    List<StandingsCriterionTotalResponse> Criteria)
{
    public static StandingsEntryResponse From(TeamStanding standing) =>
        new(
            Rank: standing.Rank,
            IsTied: standing.IsTied,
            TeamId: standing.TeamId,
            TeamName: standing.TeamName,
            AchievedScore: standing.AchievedScore,
            AvailableScore: standing.AvailableScore,
            Percentage: standing.Percentage,
            JudgesCounted: standing.JudgesCounted,
            Criteria: standing.Criteria
                .Select(c => new StandingsCriterionTotalResponse(c.CriterionId, c.CriterionName, c.AchievedScore, c.AvailableScore, c.JudgesCounted))
                .ToList());
}

/// <summary>Per-criterion total inside a standings row (published criteria only).</summary>
public sealed record StandingsCriterionTotalResponse(
    int CriterionId,
    string CriterionName,
    decimal AchievedScore,
    decimal AvailableScore,
    int JudgesCounted);
