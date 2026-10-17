namespace NasaSpaceApps.Application.Services;

public sealed record AnswerResult(bool Correct, int Points, DateTime SubmittedAt);
