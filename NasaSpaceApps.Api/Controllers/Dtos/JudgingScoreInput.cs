namespace NasaSpaceApps.Api.Controllers;

public sealed record JudgingScoreInput(int CriterionId, decimal Score, string? Comment);
