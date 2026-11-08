using Microsoft.Extensions.Configuration;
using NasaSpaceApps.Infrastructure.Services;

namespace NasaSpaceApps.Api.Tests;

public class EmailOptionsFactoryTests
{
    private static NasaSpaceApps.Application.Models.EmailOptions Build(params (string Key, string Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.ToDictionary<(string Key, string Value), string, string?>(s => s.Key, s => s.Value))
            .Build();

        return EmailOptionsFactory.FromConfiguration(configuration);
    }

    [Fact]
    public void FromConfiguration_WhenRequiredKeysMissing_ReturnsNotConfigured()
    {
        var options = Build();

        Assert.False(options.IsConfigured);
        Assert.Contains(EmailOptionsFactory.FromAddressKey, options.ConfigurationError);
        Assert.Contains(EmailOptionsFactory.SmtpPasswordKey, options.ConfigurationError);
        Assert.Equal("smtp.gmail.com", options.SmtpHost);
    }

    [Fact]
    public void FromConfiguration_WithValidSettings_IsConfigured()
    {
        var options = Build(
            (EmailOptionsFactory.SmtpHostKey, "smtp.example.com"),
            (EmailOptionsFactory.SmtpPortKey, "587"),
            (EmailOptionsFactory.SmtpUsernameKey, "user"),
            (EmailOptionsFactory.SmtpPasswordKey, "password"),
            (EmailOptionsFactory.FromAddressKey, "info@spaceappshurghada.com"),
            (EmailOptionsFactory.FromNameKey, "NASA Space Apps Hurghada"),
            (EmailOptionsFactory.UseSslKey, "true"));

        Assert.True(options.IsConfigured);
        Assert.Equal("smtp.example.com", options.SmtpHost);
        Assert.Equal(587, options.SmtpPort);
        Assert.Equal("info@spaceappshurghada.com", options.FromAddress);
        Assert.Equal("NASA Space Apps Hurghada", options.FromName);
        Assert.True(options.UseSsl);
    }

    [Fact]
    public void FromConfiguration_WhenPasswordIsMissing_ReturnsNotConfigured()
    {
        var options = Build(
            (EmailOptionsFactory.SmtpHostKey, "smtp.example.com"),
            (EmailOptionsFactory.FromAddressKey, "info@spaceappshurghada.com"),
            (EmailOptionsFactory.SmtpUsernameKey, "user"));

        Assert.False(options.IsConfigured);
        Assert.Contains(EmailOptionsFactory.SmtpPasswordKey, options.ConfigurationError);
    }

    [Fact]
    public void FromConfiguration_WhenUsernameIsMissing_UsesFromAddress()
    {
        var options = Build(
            (EmailOptionsFactory.SmtpPasswordKey, "google-app-password"),
            (EmailOptionsFactory.FromAddressKey, "info@spaceappshurghada.com"));

        Assert.True(options.IsConfigured);
        Assert.Equal("smtp.gmail.com", options.SmtpHost);
        Assert.Equal("info@spaceappshurghada.com", options.SmtpUsername);
    }

    [Fact]
    public void FromConfiguration_WhenFromAddressInvalid_ReturnsNotConfigured()
    {
        var options = Build(
            (EmailOptionsFactory.SmtpHostKey, "smtp.example.com"),
            (EmailOptionsFactory.SmtpPasswordKey, "google-app-password"),
            (EmailOptionsFactory.FromAddressKey, "not-an-email"));

        Assert.False(options.IsConfigured);
        Assert.Contains("valid email address", options.ConfigurationError);
    }

    [Fact]
    public void FromConfiguration_AppliesDefaultsAndClampsLimits()
    {
        var options = Build(
            (EmailOptionsFactory.SmtpPasswordKey, "google-app-password"),
            (EmailOptionsFactory.FromAddressKey, "info@spaceappshurghada.com"),
            (EmailOptionsFactory.MaxRecipientsKey, "999999"),
            (EmailOptionsFactory.MaxConcurrencyKey, "0"));

        Assert.Equal("smtp.gmail.com", options.SmtpHost);
        Assert.Equal(587, options.SmtpPort);
        Assert.Equal("info@spaceappshurghada.com", options.SmtpUsername);
        Assert.Equal("NASA Space Apps Challenge Hurghada", options.FromName);
        Assert.True(options.UseSsl);
        Assert.Equal(2000, options.MaxRecipients);
        Assert.Equal(4, options.MaxConcurrentSends);
    }
}
