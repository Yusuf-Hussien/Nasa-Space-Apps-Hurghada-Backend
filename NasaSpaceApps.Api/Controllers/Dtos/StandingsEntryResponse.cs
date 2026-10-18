using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>One row of the public scoreboard.</summary>
public sealed record StandingsEntryResponse(
    int? Rank,
    bool IsTied,
    int TeamId,
    string TeamName,
    decimal TotalScore,
    List<StandingsCriterionScoreResponse> Scores)
{
    public static StandingsEntryResponse From(TeamStanding standing) =>
        new(
            Rank: standing.Rank,
            IsTied: standing.IsTied,
            TeamId: standing.TeamId,
            TeamName: standing.TeamName,
            TotalScore: standing.TotalScore,
            Scores: standing.Scores
                .Select(s => new StandingsCriterionScoreResponse(s.CriterionId, s.CriterionName, s.Score, s.JudgeId))
                .ToList());
}

/// <summary>Per-criterion contribution inside a standings row.</summary>
public sealed record StandingsCriterionScoreResponse(
    int CriterionId,
    string CriterionName,
    decimal Score,
    int JudgeId);
