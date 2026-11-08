using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Api.Controllers;

public sealed record EmailFailureResponse(string Recipient, string Error);

public sealed record SendEmailResponse(
    int RequestedRecipients,
    int UniqueRecipients,
    int Sent,
    int Failed,
    IReadOnlyCollection<string> InvalidRecipients,
    IReadOnlyCollection<EmailFailureResponse> Failures)
{
    public static SendEmailResponse From(EmailSendResult result) =>
        new(
            result.RequestedRecipients,
            result.UniqueRecipients,
            result.Sent,
            result.Failed,
            result.InvalidRecipients,
            result.Failures
                .Select(failure => new EmailFailureResponse(failure.Recipient, failure.Error))
                .ToList());
}
