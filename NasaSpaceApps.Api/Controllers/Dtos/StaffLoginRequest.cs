namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Staff login payload: <c>identifier</c> is an email or username.
/// Set <c>rememberMe</c> to get a long-lived token (30 days instead of 12 hours).
/// </summary>
public sealed record StaffLoginRequest(
    string Identifier = "",
    string Password = "",
    bool RememberMe = false);
