using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Application.Tests;

public class CriterionPublicationPolicyTests
{
    [Fact]
    public void ResolveTargets_JudgeActsOnSelfOnly()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets("Judge", actorJudgeId: 3, requestedJudgeId: null, new[] { 1, 2 });
        Assert.Equal(new[] { 3 }, targets);
    }

    [Fact]
    public void ResolveTargets_JudgeWithoutJudgeLink_ReturnsEmpty()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets("Judge", actorJudgeId: null, requestedJudgeId: null, new[] { 1 });
        Assert.Empty(targets);
    }

    [Fact]
    public void ResolveTargets_AdminWithJudgeId_PublishesSpecificJudge()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets("Admin", actorJudgeId: 99, requestedJudgeId: 5, new[] { 1, 2, 3 });
        Assert.Equal(new[] { 5 }, targets);
    }

    [Fact]
    public void ResolveTargets_AdminWithoutJudgeId_PublishesAllJudgesWithScores()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets("Admin", actorJudgeId: 99, requestedJudgeId: null, new[] { 2, 1, 2 });
        Assert.Equal(new[] { 1, 2 }, targets.Distinct().OrderBy(x => x).ToArray());
    }

    [Fact]
    public void ResolveTargets_AdminWithNoScores_ReturnsEmpty()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets("Admin", actorJudgeId: 1, requestedJudgeId: null, Array.Empty<int>());
        Assert.Empty(targets);
    }

    [Fact]
    public void CanPublish_OnlyJudgeOrAdmin()
    {
        Assert.True(CriterionPublicationPolicy.CanPublish("Judge"));
        Assert.True(CriterionPublicationPolicy.CanPublish("Admin"));
        Assert.True(CriterionPublicationPolicy.CanPublish("admin"));
        Assert.False(CriterionPublicationPolicy.CanPublish("CoreTeam"));
        Assert.False(CriterionPublicationPolicy.CanPublish("Participant"));
        Assert.False(CriterionPublicationPolicy.CanPublish(null));
    }
}
