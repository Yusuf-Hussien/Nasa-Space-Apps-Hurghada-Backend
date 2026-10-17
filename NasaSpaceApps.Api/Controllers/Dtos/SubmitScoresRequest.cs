namespace NasaSpaceApps.Api.Controllers;

public sealed record SubmitScoresRequest(int TeamId, List<JudgingScoreInput> Scores);
