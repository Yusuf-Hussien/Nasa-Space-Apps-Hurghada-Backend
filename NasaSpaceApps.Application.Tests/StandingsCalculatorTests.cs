using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Tests;

public class StandingsCalculatorTests
{
    private static JudgeScore Score(int teamId, int criterionId, decimal score, decimal max = 5m, int judgeId = 1) =>
        new()
        {
            TeamId = teamId,
            CriterionId = criterionId,
            JudgeId = judgeId,
            Score = score,
            MaxScore = max,
            Criterion = new JudgingCriterion { CriterionId = criterionId, Name = $"C{criterionId}", IsPublished = true }
        };

    private static CriterionPublication Pub(int criterionId, int judgeId) =>
        new() { CriterionId = criterionId, JudgeId = judgeId };

    private static readonly Team[] Teams =
    {
        new() { TeamId = 1, Name = "Alpha" },
        new() { TeamId = 2, Name = "Beta" },
        new() { TeamId = 3, Name = "Gamma" }
    };

    [Fact]
    public void Calculate_ShouldRankByPercentage_NotRawScore()
    {
        // Alpha: 4/5 = 80%. Beta: 15/20 = 75%. Higher raw score but lower percentage.
        var scores = new List<JudgeScore>
        {
            Score(1, 1, 4m),
            Score(2, 1, 15m, 20m)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);

        Assert.Equal("Alpha", standings[0].TeamName);
        Assert.Equal(80m, standings[0].Percentage);
        Assert.Equal(1, standings[0].Rank);

        Assert.Equal("Beta", standings[1].TeamName);
        Assert.Equal(75m, standings[1].Percentage);
        Assert.Equal(2, standings[1].Rank);
    }

    [Fact]
    public void Calculate_Should_TieTeamsWithEqualPercentage()
    {
        var scores = new List<JudgeScore>
        {
            Score(1, 1, 4m),
            Score(2, 1, 8m, 10m)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);

        Assert.Equal(1, standings[0].Rank);
        Assert.Equal(1, standings[1].Rank);
        Assert.True(standings[0].IsTied);
        Assert.True(standings[1].IsTied);
    }

    [Fact]
    public void Calculate_Should_IgnoreUnpublishedCriteria()
    {
        var score = Score(1, 1, 5m);
        score.Criterion!.IsPublished = true;

        var standings = StandingsCalculator.Calculate(
            new List<JudgeScore> { score },
            Teams,
            new List<CriterionPublication>());
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(0m, alpha.AchievedScore);
        Assert.Equal(0m, alpha.AvailableScore);
        Assert.Null(alpha.Rank);
    }

    [Fact]
    public void Calculate_Should_IncludeUnscoredTeams_WithNoRank()
    {
        var standings = StandingsCalculator.Calculate(
            new List<JudgeScore> { Score(1, 1, 5m) },
            Teams,
            new List<CriterionPublication> { Pub(1, 1) });

        Assert.Equal(3, standings.Count);

        var scored = standings.Single(row => row.TeamId == 1);
        Assert.Equal(1, scored.Rank);
        Assert.Equal(100m, scored.Percentage);

        var unscored = standings.Single(row => row.TeamId == 2);
        Assert.Null(unscored.Rank);
        Assert.False(unscored.IsTied);
        Assert.Equal(0m, unscored.Percentage);
    }

    [Fact]
    public void Calculate_ShouldAverageJudges_WhenBothPublish()
    {
        // Judge 1: 4.5/5, Judge 2: 4/5 -> average 4.25/5 = 85%
        var scores = new List<JudgeScore>
        {
            Score(1, 1, 4.5m, judgeId: 1),
            Score(1, 1, 4m, judgeId: 2)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1), Pub(1, 2) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(4.25m, alpha.AchievedScore);
        Assert.Equal(5m, alpha.AvailableScore);
        Assert.Equal(85m, alpha.Percentage);
        Assert.Equal(2, alpha.JudgesCounted);
        Assert.Equal(2, alpha.Criteria[0].JudgesCounted);
    }

    [Fact]
    public void Calculate_ShouldOnlyCountPublishedJudgeScores()
    {
        // Judge 1 published (counts), Judge 2 not published yet (ignored)
        var scores = new List<JudgeScore>
        {
            Score(1, 1, 4.5m, judgeId: 1),
            Score(1, 1, 2.5m, judgeId: 2)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(90m, alpha.Percentage);
        Assert.Equal(1, alpha.JudgesCounted);
    }

    [Fact]
    public void Calculate_ShouldUseTheSnapshottedMaxScore()
    {
        var score = Score(1, 1, 4m);
        score.Criterion!.MaxScore = 100m;

        var standings = StandingsCalculator.Calculate(
            new List<JudgeScore> { score },
            Teams,
            new List<CriterionPublication> { Pub(1, 1) });
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(5m, alpha.AvailableScore);
        Assert.Equal(80m, alpha.Percentage);
    }

    [Fact]
    public void Calculate_Should_ReturnNoRows_WhenThereAreNoTeams()
    {
        var standings = StandingsCalculator.Calculate(
            new List<JudgeScore>(),
            new List<Team>(),
            new List<CriterionPublication>());

        Assert.Empty(standings);
    }
}
