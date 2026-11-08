namespace NasaSpaceApps.Application.Models;

public sealed record EmailFailure(string Recipient, string Error);

public sealed record EmailSendResult(
    int RequestedRecipients,
    int UniqueRecipients,
    int Sent,
    int Failed,
    IReadOnlyCollection<string> InvalidRecipients,
    IReadOnlyCollection<EmailFailure> Failures);
