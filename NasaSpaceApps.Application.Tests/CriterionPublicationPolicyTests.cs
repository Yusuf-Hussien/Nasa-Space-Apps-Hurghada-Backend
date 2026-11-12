using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Application.Tests;

public class CriterionPublicationPolicyTests
{
    [Fact]
    public void ResolveTargets_JudgeActsOnOwnScoredTeamsOnly()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Judge",
            actorJudgeId: 3,
            requestedJudgeId: null,
            requestedTeamId: null,
            new[] { (1, 10), (2, 10), (3, 10), (3, 20) });

        Assert.Equal(new[] { (3, 10), (3, 20) }, targets.OrderBy(target => target.TeamId).ToArray());
    }

    [Fact]
    public void ResolveTargets_JudgeCanTargetOwnTeam()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Judge",
            actorJudgeId: 3,
            requestedJudgeId: null,
            requestedTeamId: 20,
            new[] { (3, 10), (3, 20) });

        Assert.Equal(new[] { (3, 20) }, targets);
    }

    [Fact]
    public void ResolveTargets_JudgeWithoutJudgeLink_ReturnsEmpty()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Judge",
            actorJudgeId: null,
            requestedJudgeId: null,
            requestedTeamId: null,
            new[] { (1, 10) });

        Assert.Empty(targets);
    }

    [Fact]
    public void ResolveTargets_AdminWithJudgeId_PublishesSpecificJudge()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Admin",
            actorJudgeId: 99,
            requestedJudgeId: 5,
            requestedTeamId: null,
            new[] { (1, 10), (2, 10), (5, 10), (5, 20) });

        Assert.Equal(new[] { (5, 10), (5, 20) }, targets.OrderBy(target => target.TeamId).ToArray());
    }

    [Fact]
    public void ResolveTargets_AdminWithTeamId_PublishesSpecificTeam()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Admin",
            actorJudgeId: null,
            requestedJudgeId: null,
            requestedTeamId: 20,
            new[] { (1, 10), (2, 20), (5, 20) });

        Assert.Equal(new[] { (2, 20), (5, 20) }, targets.OrderBy(target => target.JudgeId).ToArray());
    }

    [Fact]
    public void ResolveTargets_AdminWithJudgeAndTeam_PublishesExactPair()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Admin",
            actorJudgeId: null,
            requestedJudgeId: 5,
            requestedTeamId: 20,
            new[] { (5, 10), (5, 20), (6, 20) });

        Assert.Equal(new[] { (5, 20) }, targets);
    }

    [Fact]
    public void ResolveTargets_AdminWithoutFilters_PublishesAllScoredPairs()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Admin",
            actorJudgeId: 99,
            requestedJudgeId: null,
            requestedTeamId: null,
            new[] { (2, 10), (1, 10), (2, 10), (2, 20) });

        Assert.Equal(
            new[] { (1, 10), (2, 10), (2, 20) },
            targets.OrderBy(target => target.JudgeId).ThenBy(target => target.TeamId).ToArray());
    }

    [Fact]
    public void ResolveTargets_AdminWithNoScores_ReturnsEmpty()
    {
        var targets = CriterionPublicationPolicy.ResolveTargets(
            "Admin",
            actorJudgeId: 1,
            requestedJudgeId: null,
            requestedTeamId: null,
            Array.Empty<(int JudgeId, int TeamId)>());

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
