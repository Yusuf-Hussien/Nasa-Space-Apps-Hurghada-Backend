using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// HTTP response shape for a quiz question.
/// <paramref name="CorrectAnswer"/> is null unless the caller is allowed to see it.
/// </summary>
public sealed record QuestionResponse(
    int QuestionId,
    int ChallengeId,
    string Type,
    string Text,
    string? Options,
    string? CorrectAnswer,
    bool IsPublished,
    DateTime? PublishedAt,
    int Points,
    bool IsActive)
{
    public static QuestionResponse From(Question question, bool includeAnswer) =>
        new(
            QuestionId: question.QuestionId,
            ChallengeId: question.ChallengeId,
            Type: question.Type,
            Text: question.Text,
            Options: question.Options,
            CorrectAnswer: includeAnswer ? question.CorrectAnswer : null,
            IsPublished: question.IsPublished,
            PublishedAt: question.PublishedAt,
            Points: question.Points,
            IsActive: question.IsActive);
}
