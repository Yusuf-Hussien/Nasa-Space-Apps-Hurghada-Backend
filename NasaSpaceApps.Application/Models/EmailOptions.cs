namespace NasaSpaceApps.Application.Models;

/// <summary>
/// SMTP configuration for the outbound email service. Values are read from
/// environment variables at startup so production credentials never live in
/// source control.
/// </summary>
public sealed record EmailOptions(
    string SmtpHost,
    int SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    string FromAddress,
    string FromName,
    bool UseSsl,
    int MaxRecipients,
    int MaxConcurrentSends,
    bool IsConfigured,
    string? ConfigurationError);
