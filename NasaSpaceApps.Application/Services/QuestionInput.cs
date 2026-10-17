namespace NasaSpaceApps.Application.Services;

public sealed record QuestionInput(
    int ChallengeId,
    string Type,
    string Text,
    string? Options,
    string? CorrectAnswer,
    int Points);
