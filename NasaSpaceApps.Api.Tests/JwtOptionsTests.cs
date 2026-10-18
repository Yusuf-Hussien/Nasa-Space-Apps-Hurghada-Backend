using Microsoft.Extensions.Configuration;
using NasaSpaceApps.Infrastructure.Services;

namespace NasaSpaceApps.Api.Tests;

public class JwtOptionsTests
{
    private const string ValidKey = "Unit-Test-Signing-Key-That-Is-Long-Enough-32";

    private static JwtOptions Build(params (string Key, string Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.ToDictionary< (string Key, string Value), string, string?>(s => s.Key, s => s.Value))
            .Build();

        return JwtOptions.FromConfiguration(configuration);
    }

    [Fact]
    public void FromConfiguration_Should_Throw_WhenKeyIsMissing()
    {
        // Fail-fast: a missing key must surface at startup, not as a 500 mid-login.
        var ex = Assert.Throws<InvalidOperationException>(() => Build());
        Assert.Contains("signing key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromConfiguration_Should_Throw_WhenKeyIsEmptyString()
    {
        // Regression guard: an env var set to "" used to override appsettings
        // and produced an opaque 500 on every login.
        var ex = Assert.Throws<InvalidOperationException>(
            () => Build(("Jwt:Key", "")));

        Assert.Contains("too short", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromConfiguration_Should_Throw_WhenKeyIsTooShort()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Build(("Jwt:Key", "short")));

        Assert.Contains("32", ex.Message);
    }

    [Fact]
    public void FromConfiguration_Should_TrimAndAcceptAValidKey()
    {
        var options = Build(("Jwt:Key", $"  {ValidKey}  "));

        Assert.Equal(ValidKey, options.Key);
    }

    [Fact]
    public void FromConfiguration_Should_FallBackToDefaults_ForIssuerAudienceAndExpiry()
    {
        var options = Build(("Jwt:Key", ValidKey));

        Assert.Equal("NasaSpaceApps", options.Issuer);
        Assert.Equal("NasaSpaceApps.Client", options.Audience);
        Assert.Equal(720, options.ExpiryMinutes);
        Assert.Equal(43200, options.RememberMeExpiryMinutes);
    }

    [Fact]
    public void FromConfiguration_Should_HonourConfiguredExpiry()
    {
        var options = Build(
            ("Jwt:Key", ValidKey),
            ("Jwt:ExpiryMinutes", "30"),
            ("Jwt:RememberMeExpiryMinutes", "10080"));

        Assert.Equal(30, options.ExpiryMinutes);
        Assert.Equal(10080, options.RememberMeExpiryMinutes);
    }

    [Fact]
    public void FromConfiguration_Should_IgnoreNonPositiveExpiry()
    {
        var options = Build(("Jwt:Key", ValidKey), ("Jwt:ExpiryMinutes", "0"));

        Assert.Equal(720, options.ExpiryMinutes);
    }
}
