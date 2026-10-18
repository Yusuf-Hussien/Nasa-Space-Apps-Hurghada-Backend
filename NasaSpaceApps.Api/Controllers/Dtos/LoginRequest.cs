namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Login payload shared by all three login routes:
/// staff send <c>identifier</c> (email or username) + <c>password</c>;
/// participants send <c>teamId</c> + <c>password</c>.
/// Set <c>rememberMe</c> to get a long-lived token.
/// </summary>
public sealed record LoginRequest(
    string? Identifier = null,
    int? TeamId = null,
    string Password = "",
    bool RememberMe = false);
