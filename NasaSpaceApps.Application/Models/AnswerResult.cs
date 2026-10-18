namespace NasaSpaceApps.Application.Models;

public sealed record AnswerResult(bool Correct, int Points, DateTime SubmittedAt);
