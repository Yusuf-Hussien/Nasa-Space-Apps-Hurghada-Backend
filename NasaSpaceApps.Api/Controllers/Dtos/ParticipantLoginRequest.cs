namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Participant login payload: <c>teamId</c> identifies the team.
/// Set <c>rememberMe</c> to get a long-lived token (30 days instead of 12 hours).
/// </summary>
public sealed record ParticipantLoginRequest(
    int TeamId = 0,
    string Password = "",
    bool RememberMe = false);
