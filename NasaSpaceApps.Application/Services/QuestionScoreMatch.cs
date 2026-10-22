using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

/// <summary>
/// Decides whether an already-saved score row is the same score as an incoming
/// one, so a judge re-submitting replaces their score instead of adding a
/// duplicate that would double-count on the scoreboard.
/// </summary>
public static class QuestionScoreMatch
{
    /// <summary>
    /// The identity of a score: the same team, criterion, judge and question.
    /// Catalogue questions are matched by <see cref="QuestionScore.CriterionQuestionId"/>;
    /// ad-hoc "other" questions (no catalogue id) are matched by their name.
    /// </summary>
    public static bool Matches(
        QuestionScore existing,
        int teamId,
        int criterionId,
        int? criterionQuestionId,
        int judgeId,
        string? questionName)
    {
        if (existing.TeamId != teamId ||
            existing.CriterionId != criterionId ||
            existing.JudgeId != judgeId)
        {
            return false;
        }

        if (criterionQuestionId.HasValue)
        {
            return existing.CriterionQuestionId == criterionQuestionId.Value;
        }

        // Ad-hoc question: identified by its name (null/empty names all collapse
        // into one unnamed ad-hoc slot per team + criterion + judge).
        return existing.CriterionQuestionId is null &&
               Normalize(existing.QuestionName) == Normalize(questionName);
    }

    /// <summary>Finds the existing score the incoming one should replace, if any.</summary>
    public static QuestionScore? FindExisting(
        IEnumerable<QuestionScore> existingScores,
        int teamId,
        int criterionId,
        int? criterionQuestionId,
        int judgeId,
        string? questionName)
    {
        return existingScores.FirstOrDefault(score =>
            Matches(score, teamId, criterionId, criterionQuestionId, judgeId, questionName));
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
