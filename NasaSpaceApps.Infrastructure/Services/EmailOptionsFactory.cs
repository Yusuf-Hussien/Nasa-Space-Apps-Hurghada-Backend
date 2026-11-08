using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Infrastructure.Services;

/// <summary>
/// Reads the EMAIL_* environment variables once at startup. Google/Gmail SMTP
/// is the default, so only EMAIL_FROM_ADDRESS and EMAIL_SMTP_PASSWORD are
/// required. Missing or invalid values do not crash the API; the email
/// endpoint returns email.not_configured instead.
/// </summary>
public static class EmailOptionsFactory
{
    public const string SmtpHostKey = "EMAIL_SMTP_HOST";
    public const string SmtpPortKey = "EMAIL_SMTP_PORT";
    public const string SmtpUsernameKey = "EMAIL_SMTP_USERNAME";
    public const string SmtpPasswordKey = "EMAIL_SMTP_PASSWORD";
    public const string FromAddressKey = "EMAIL_FROM_ADDRESS";
    public const string FromNameKey = "EMAIL_FROM_NAME";
    public const string UseSslKey = "EMAIL_USE_SSL";
    public const string MaxRecipientsKey = "EMAIL_MAX_RECIPIENTS";
    public const string MaxConcurrencyKey = "EMAIL_MAX_CONCURRENCY";

    private const string DefaultSmtpHost = "smtp.gmail.com";
    private const int DefaultPort = 587;
    private const string DefaultFromName = "NASA Space Apps Challenge Hurghada";
    private const int DefaultMaxRecipients = 2000;
    private const int DefaultMaxConcurrency = 4;

    public static EmailOptions FromConfiguration(IConfiguration configuration)
    {
        var errors = new List<string>();

        var smtpHost = Read(configuration, SmtpHostKey);
        if (smtpHost.Length == 0)
        {
            smtpHost = DefaultSmtpHost;
        }

        var fromAddress = Read(configuration, FromAddressKey);
        if (fromAddress.Length == 0)
        {
            errors.Add($"{FromAddressKey} is required.");
        }
        else if (!MailAddress.TryCreate(fromAddress, out _))
        {
            errors.Add($"{FromAddressKey} is not a valid email address.");
        }

        var port = DefaultPort;
        var rawPort = Read(configuration, SmtpPortKey);
        if (rawPort.Length > 0 &&
            (!int.TryParse(rawPort, out port) || port < 1 || port > 65535))
        {
            errors.Add($"{SmtpPortKey} must be a number between 1 and 65535.");
            port = DefaultPort;
        }

        var smtpUsername = Read(configuration, SmtpUsernameKey);
        var smtpPassword = Read(configuration, SmtpPasswordKey);
        if (smtpPassword.Length == 0)
        {
            errors.Add($"{SmtpPasswordKey} is required (use a Google App Password).");
        }
        else if (smtpUsername.Length == 0)
        {
            smtpUsername = fromAddress;
        }

        var fromName = Read(configuration, FromNameKey);
        if (fromName.Length == 0)
        {
            fromName = DefaultFromName;
        }

        return new EmailOptions(
            SmtpHost: smtpHost,
            SmtpPort: port,
            SmtpUsername: smtpUsername.Length == 0 ? null : smtpUsername,
            SmtpPassword: smtpPassword.Length == 0 ? null : smtpPassword,
            FromAddress: fromAddress,
            FromName: fromName,
            UseSsl: ReadBool(configuration, UseSslKey, fallback: true),
            MaxRecipients: ReadInt(configuration, MaxRecipientsKey, DefaultMaxRecipients, min: 1, max: 10000),
            MaxConcurrentSends: ReadInt(configuration, MaxConcurrencyKey, DefaultMaxConcurrency, min: 1, max: 20),
            IsConfigured: errors.Count == 0,
            ConfigurationError: errors.Count == 0 ? null : string.Join(" ", errors));
    }

    private static string Read(IConfiguration configuration, string key) =>
        configuration[key]?.Trim() ?? string.Empty;

    private static bool ReadBool(IConfiguration configuration, string key, bool fallback)
    {
        var raw = Read(configuration, key);
        if (raw.Length == 0)
        {
            return fallback;
        }

        if (bool.TryParse(raw, out var value))
        {
            return value;
        }

        return raw.ToLowerInvariant() switch
        {
            "1" or "yes" or "on" => true,
            "0" or "no" or "off" => false,
            _ => fallback
        };
    }

    private static int ReadInt(IConfiguration configuration, string key, int fallback, int min, int max)
    {
        var raw = Read(configuration, key);
        return int.TryParse(raw, out var value) && value >= min && value <= max
            ? value
            : fallback;
    }
}
