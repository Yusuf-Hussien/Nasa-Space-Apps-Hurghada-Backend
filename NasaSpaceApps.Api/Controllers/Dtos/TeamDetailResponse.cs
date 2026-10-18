using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a single team (staff-only view).</summary>
public sealed record TeamDetailResponse(
    int TeamId,
    string Name,
    string Username,
    string Status,
    int ChallengeId)
{
    public static TeamDetailResponse From(Team team) =>
        new(team.TeamId, team.Name, team.Username, team.Status, team.ChallengeId);
}
