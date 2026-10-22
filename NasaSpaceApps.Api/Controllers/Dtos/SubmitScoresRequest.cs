using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Api.Controllers;

public sealed record SubmitScoresRequest(int TeamId, List<QuestionScoreInput> Scores);
