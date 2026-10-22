using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a judging criterion.</summary>
public sealed record JudgingCriterionResponse(
    int CriterionId,
    int? ChallengeId,
    string Name,
    string? Description,
    decimal MaxScore,
    decimal Weight,
    bool IsPublished,
    DateTime? PublishedAt,
    int PublishedJudgeCount,
    bool? IsPublishedByCaller,
    List<CriterionQuestionResponse> Questions)
{
    public static JudgingCriterionResponse From(JudgingCriterion criterion) =>
        From(criterion, null, null);

    public static JudgingCriterionResponse From(JudgingCriterion criterion, IReadOnlyCollection<CriterionPublication>? publications, int? callerJudgeId)
    {
        IReadOnlyCollection<CriterionPublication> pubs;
        if (publications is not null)
        {
            pubs = publications;
        }
        else if (criterion.Publications is not null)
        {
            pubs = criterion.Publications.ToList();
        }
        else
        {
            pubs = Array.Empty<CriterionPublication>();
        }

        var criterionPublications = pubs.Where(publication => publication.CriterionId == criterion.CriterionId).ToList();
        var publishedCount = criterionPublications.Count;
        bool? publishedByCaller = null;
        if (callerJudgeId.HasValue)
        {
            publishedByCaller = criterionPublications.Any(publication => publication.JudgeId == callerJudgeId.Value);
        }

        return new(
            criterion.CriterionId,
            criterion.ChallengeId,
            criterion.Name,
            criterion.Description,
            criterion.MaxScore,
            criterion.Weight,
            publishedCount > 0,
            publishedCount == 0 ? null : criterionPublications.Max(publication => publication.PublishedAt),
            publishedCount,
            publishedByCaller,
            criterion.Questions
                .OrderBy(question => question.DisplayOrder)
                .Select(CriterionQuestionResponse.From)
                .ToList());
    }
}

/// <summary>HTTP response shape for an article/reference question on a criterion.</summary>
public sealed record CriterionQuestionResponse(
    int CriterionQuestionId,
    int CriterionId,
    string Text,
    int DisplayOrder,
    bool IsActive)
{
    public static CriterionQuestionResponse From(CriterionQuestion question) =>
        new(
            question.CriterionQuestionId,
            question.CriterionId,
            question.Text,
            question.DisplayOrder,
            question.IsActive);
}
