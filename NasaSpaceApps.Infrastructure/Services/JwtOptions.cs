using Microsoft.Extensions.Configuration;

namespace NasaSpaceApps.Infrastructure.Services;

/// <summary>
/// Single source of truth for JWT settings. Read once, validated once, at startup —
/// so a bad value fails the app immediately with a clear message instead of
/// surfacing as an opaque 500 halfway through a login request.
/// </summary>
public sealed record JwtOptions(
    string Key,
    string Issuer,
    string Audience,
    int ExpiryMinutes,
    int RememberMeExpiryMinutes)
{
    /// <summary>HS256 needs >= 128 bits; we require 256 bits (32 chars) to stay safe.</summary>
    private const int MinimumKeyLength = 32;
    private const int DefaultExpiryMinutes = 720; // 12 hours — event-long hackathon session
    private const int DefaultRememberMeExpiryMinutes = 43200; // 30 days

    public static JwtOptions FromConfiguration(IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"]?.Trim() ?? string.Empty;
        if (key.Length < MinimumKeyLength)
        {
            throw new InvalidOperationException(
                "JWT signing key is missing or too short " +
                $"(got {key.Length} characters, need at least {MinimumKeyLength}). " +
                "Set 'Jwt:Key' to a random string, e.g. `openssl rand -hex 32`.");
        }

        var issuer = configuration["Jwt:Issuer"]?.Trim();
        var audience = configuration["Jwt:Audience"]?.Trim();

        return new JwtOptions(
            Key: key,
            Issuer: string.IsNullOrEmpty(issuer) ? "NasaSpaceApps" : issuer,
            Audience: string.IsNullOrEmpty(audience) ? "NasaSpaceApps.Client" : audience,
            ExpiryMinutes: ReadPositive(configuration, "Jwt:ExpiryMinutes", DefaultExpiryMinutes),
            RememberMeExpiryMinutes: ReadPositive(configuration, "Jwt:RememberMeExpiryMinutes", DefaultRememberMeExpiryMinutes));
    }

    private static int ReadPositive(IConfiguration configuration, string key, int fallback) =>
        int.TryParse(configuration[key], out var value) && value > 0 ? value : fallback;
}
