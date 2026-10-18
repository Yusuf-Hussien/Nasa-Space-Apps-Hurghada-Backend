using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a judging criterion.</summary>
public sealed record JudgingCriterionResponse(
    int CriterionId,
    string Name,
    string? Description,
    decimal MaxScore,
    decimal Weight)
{
    public static JudgingCriterionResponse From(JudgingCriterion criterion) =>
        new(criterion.CriterionId, criterion.Name, criterion.Description, criterion.MaxScore, criterion.Weight);
}
