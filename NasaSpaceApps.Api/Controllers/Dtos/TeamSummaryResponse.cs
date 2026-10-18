using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// HTTP response shape for a team in list view.
/// <paramref name="Username"/> is only populated for staff callers.
/// </summary>
public sealed record TeamSummaryResponse(
    int TeamId,
    string Name,
    string? Username,
    string Status)
{
    public static TeamSummaryResponse From(Team team, bool includeUsername) =>
        new(
            TeamId: team.TeamId,
            Name: team.Name,
            Username: includeUsername ? team.Username : null,
            Status: team.Status);
}
