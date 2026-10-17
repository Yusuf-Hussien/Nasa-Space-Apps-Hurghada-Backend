namespace NasaSpaceApps.Api.Controllers;

public sealed record QuestionInputDto(
    int ChallengeId,
    string Type,
    string Text,
    string? Options,
    string? CorrectAnswer,
    int Points);
