using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

/// <summary>
/// Participants can only see an active question while the competition is running
/// and the question has been released by an admin.
/// </summary>
public static class GameVisibilityPolicy
{
    public static bool CanParticipantSee(GameState? state, Question question) =>
        state?.IsStarted == true && question.IsActive && question.IsPublished;
}
