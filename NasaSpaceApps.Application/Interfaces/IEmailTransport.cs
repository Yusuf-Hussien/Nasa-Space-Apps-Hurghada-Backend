using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Application.Interfaces;

/// <summary>Low-level email transport. Kept separate so the bulk-send logic is testable without SMTP.</summary>
public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
