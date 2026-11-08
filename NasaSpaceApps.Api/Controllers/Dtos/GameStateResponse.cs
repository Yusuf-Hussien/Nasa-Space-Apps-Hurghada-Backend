using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Public competition state. Participants use this to show the waiting screen
/// before an admin starts the game.
/// </summary>
public sealed record GameStateResponse(
    bool IsStarted,
    DateTime? StartedAt,
    DateTime? StoppedAt,
    DateTime UpdatedAt)
{
    public static GameStateResponse From(GameState state) =>
        new(
            IsStarted: state.IsStarted,
            StartedAt: state.StartedAt,
            StoppedAt: state.StoppedAt,
            UpdatedAt: state.UpdatedAt);
}
