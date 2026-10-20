namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Participant login payload: <c>username</c> is the team's unique login username.
/// Set <c>rememberMe</c> to get a long-lived token (30 days instead of 12 hours).
/// </summary>
public sealed record ParticipantLoginRequest(
    string Username = "",
    string Password = "",
    bool RememberMe = false);
