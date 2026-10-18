namespace NasaSpaceApps.Application.Models;

/// <summary>Create/update payload for a quiz question.</summary>
public sealed record QuestionInput(
    int ChallengeId,
    string Type,
    string Text,
    string? Options,
    string? CorrectAnswer,
    int Points);
