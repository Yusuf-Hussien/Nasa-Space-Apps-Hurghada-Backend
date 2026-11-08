using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Tests;

public class GameVisibilityPolicyTests
{
    private static GameState StartedState() => new() { IsStarted = true };
    private static GameState StoppedState() => new() { IsStarted = false };
    private static Question Question(bool published = true, bool active = true) =>
        new() { IsPublished = published, IsActive = active };

    [Fact]
    public void CanParticipantSee_StartedAndReleased_ReturnsTrue()
    {
        Assert.True(GameVisibilityPolicy.CanParticipantSee(StartedState(), Question()));
    }

    [Fact]
    public void CanParticipantSee_GameNotStarted_ReturnsFalse()
    {
        Assert.False(GameVisibilityPolicy.CanParticipantSee(StoppedState(), Question()));
    }

    [Fact]
    public void CanParticipantSee_MissingState_ReturnsFalse()
    {
        Assert.False(GameVisibilityPolicy.CanParticipantSee(null, Question()));
    }

    [Fact]
    public void CanParticipantSee_QuestionNotPublished_ReturnsFalse()
    {
        Assert.False(GameVisibilityPolicy.CanParticipantSee(StartedState(), Question(published: false)));
    }

    [Fact]
    public void CanParticipantSee_InactiveQuestion_ReturnsFalse()
    {
        Assert.False(GameVisibilityPolicy.CanParticipantSee(StartedState(), Question(active: false)));
    }
}
