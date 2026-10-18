using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a judge's score on a team.</summary>
public sealed record JudgeScoreResponse(
    int ScoreId,
    int TeamId,
    int? JudgeId,
    int CriterionId,
    string? CriterionName,
    decimal Score,
    string? Comment,
    DateTime? Timestamp)
{
    public static JudgeScoreResponse From(JudgeScore score, bool includeJudge = true) =>
        new(
            ScoreId: score.ScoreId,
            TeamId: score.TeamId,
            JudgeId: includeJudge ? score.JudgeId : null,
            CriterionId: score.CriterionId,
            CriterionName: score.Criterion?.Name,
            Score: score.Score,
            Comment: score.Comment,
            Timestamp: score.Timestamp);
}
