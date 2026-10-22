namespace NasaSpaceApps.Application.Models;

/// <summary>Create/update payload for a judging criterion.</summary>
public sealed record JudgingCriterionInput(
    string Name,
    string? Description,
    decimal MaxScore,
    decimal Weight,
    int? ChallengeId = null,
    List<CriterionQuestionInput>? Questions = null);

/// <summary>An article/reference question attached to a criterion.</summary>
public sealed record CriterionQuestionInput(
    string Text,
    int DisplayOrder = 0);
