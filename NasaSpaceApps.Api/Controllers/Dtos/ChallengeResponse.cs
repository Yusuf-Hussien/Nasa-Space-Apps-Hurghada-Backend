using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a hackathon challenge.</summary>
public sealed record ChallengeResponse(
    int ChallengeId,
    string Title,
    string? Description,
    string? Source,
    string? Season,
    string? Image)
{
    public static ChallengeResponse From(Challenge challenge) =>
        new(challenge.ChallengeId, challenge.Title, challenge.Description, challenge.Source, challenge.Season, challenge.Image);
}
