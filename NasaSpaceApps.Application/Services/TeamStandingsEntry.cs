namespace NasaSpaceApps.Application.Services;

public sealed record TeamStandingsEntry(
    int TeamId,
    string TeamName,
    decimal TotalScore,
    List<TeamStandingsScore> Scores);

public sealed record TeamStandingsScore(
    int CriterionId,
    string CriterionName,
    decimal Score,
    int JudgeId);
