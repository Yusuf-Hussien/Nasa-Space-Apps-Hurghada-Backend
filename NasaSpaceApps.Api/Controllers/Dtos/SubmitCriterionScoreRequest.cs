namespace NasaSpaceApps.Api.Controllers;

/// <summary>One criterion score for a team, submitted and published together.</summary>
public sealed record SubmitCriterionScoreRequest(int TeamId, decimal Score, string? Comment = null);

/// <summary>Result of saving and publishing one criterion score.</summary>
public sealed record SubmitCriterionScoreResponse(JudgeScoreResponse Score, JudgingCriterionResponse Criterion);
