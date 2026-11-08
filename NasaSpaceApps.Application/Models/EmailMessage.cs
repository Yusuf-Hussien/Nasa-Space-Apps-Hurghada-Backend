namespace NasaSpaceApps.Application.Models;

/// <summary>A fully rendered, single-recipient email ready for transport.</summary>
public sealed record EmailMessage(
    string Recipient,
    string Subject,
    string HtmlBody,
    string TextBody,
    string FromAddress,
    string FromName);
