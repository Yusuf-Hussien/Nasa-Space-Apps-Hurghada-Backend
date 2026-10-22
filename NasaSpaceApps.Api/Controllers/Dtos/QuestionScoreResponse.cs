using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a judge's score on a single question.</summary>
public sealed record QuestionScoreResponse(
    int QuestionScoreId,
    int TeamId,
    int CriterionId,
    int? CriterionQuestionId,
    string? QuestionName,
    decimal MaxScore,
    decimal Score,
    bool Skipped,
    string? Comment,
    DateTime Timestamp)
{
    public static QuestionScoreResponse From(QuestionScore score) =>
        new(
            score.QuestionScoreId,
            score.TeamId,
            score.CriterionId,
            score.CriterionQuestionId,
            score.QuestionName,
            score.MaxScore,
            score.Score,
            score.Skipped,
            score.Comment,
            score.Timestamp);
}
