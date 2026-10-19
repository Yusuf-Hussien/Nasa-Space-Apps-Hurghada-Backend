using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// HTTP response shape for a successful participant login.
/// Deliberately has no staff-only fields such as <c>email</c>, <c>staffId</c> or <c>judgeId</c>.
/// </summary>
public sealed record ParticipantLoginResponse(
    string AccessToken,
    string Role,
    string Name,
    int TeamId,
    string Username)
{
    public static ParticipantLoginResponse From(LoginResponse value) =>
        new(
            AccessToken: value.AccessToken,
            Role: value.Role,
            Name: value.Name,
            TeamId: value.TeamId ?? 0,
            Username: value.Username ?? string.Empty);
}
