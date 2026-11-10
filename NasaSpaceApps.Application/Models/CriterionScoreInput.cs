namespace NasaSpaceApps.Application.Models;

/// <summary>One criterion-level score for a team.</summary>
public sealed record CriterionScoreInput(
    int CriterionId,
    decimal Score,
    string? Comment = null);
