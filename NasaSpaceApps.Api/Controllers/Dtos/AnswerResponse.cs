using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Participant-safe answer acknowledgement. Correctness and points are
/// intentionally hidden until an admin reveals them.
/// </summary>
public sealed record AnswerResponse(DateTime SubmittedAt)
{
    public static AnswerResponse From(AnswerResult result) =>
        new(SubmittedAt: result.SubmittedAt);
}
