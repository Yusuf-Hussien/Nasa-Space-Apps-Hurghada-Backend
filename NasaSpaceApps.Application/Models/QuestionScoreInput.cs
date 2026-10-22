namespace NasaSpaceApps.Application.Models;

/// <summary>
/// One per-question score entry. <see cref="CriterionQuestionId"/> may be null for
/// an ad-hoc "other" question, in which case <see cref="Name"/> describes it and
/// <see cref="MaxScore"/> is required. <see cref="Skipped"/> excludes it from the total.
/// </summary>
public sealed record QuestionScoreInput(
    int CriterionId,
    int? CriterionQuestionId,
    string? Name,
    decimal? MaxScore,
    decimal Score,
    bool Skipped = false,
    string? Comment = null);
