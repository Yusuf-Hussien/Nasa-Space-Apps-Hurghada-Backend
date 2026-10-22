using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Tests;

public class StandingsCalculatorTests
{
    private static QuestionScore Score(int teamId, int criterionId, decimal score, decimal max, bool skipped = false, int judgeId = 1) =>
        new()
        {
            TeamId = teamId,
            CriterionId = criterionId,
            JudgeId = judgeId,
            Score = score,
            MaxScore = max,
            Skipped = skipped,
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
        // Alpha: 8/10 = 80%. Beta: 15/20 = 75%. Higher raw score but lower percentage.
        var scores = new List<QuestionScore>
        {
            Score(1, 1, 8m, 10m),
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
        var scores = new List<QuestionScore>
        {
            Score(1, 1, 8m, 10m),
            Score(2, 1, 16m, 20m)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);

        Assert.Equal(1, standings[0].Rank);
        Assert.Equal(1, standings[1].Rank);
        Assert.True(standings[0].IsTied);
        Assert.True(standings[1].IsTied);
    }

    [Fact]
    public void Calculate_Should_ExcludeSkippedQuestions_FromBothScores()
    {
        // Skipped question (0/10) must not count against the team.
        var scores = new List<QuestionScore>
        {
            Score(1, 1, 5m, 10m),
            Score(1, 2, 0m, 10m, skipped: true)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1), Pub(2, 1) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(5m, alpha.AchievedScore);
        Assert.Equal(10m, alpha.AvailableScore);
        Assert.Equal(50m, alpha.Percentage);
    }

    [Fact]
    public void Calculate_Should_IgnoreUnpublishedCriteria()
    {
        var score = Score(1, 1, 5m, 10m);
        // No publication => ignored even if criterion.IsPublished is true
        score.Criterion!.IsPublished = true;

        var standings = StandingsCalculator.Calculate(new List<QuestionScore> { score }, Teams, new List<CriterionPublication>());
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(0m, alpha.AchievedScore);
        Assert.Equal(0m, alpha.AvailableScore);
        Assert.Null(alpha.Rank);
    }

    [Fact]
    public void Calculate_Should_IncludeUnscoredTeams_WithNoRank()
    {
        var standings = StandingsCalculator.Calculate(
            new List<QuestionScore> { Score(1, 1, 10m, 10m) },
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
        // Judge 1: 9/10, Judge 2: 8/10 -> average 8.5/10 = 85%
        var scores = new List<QuestionScore>
        {
            Score(1, 1, 9m, 10m, judgeId: 1),
            Score(1, 1, 8m, 10m, judgeId: 2)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1), Pub(1, 2) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(8.5m, alpha.AchievedScore);
        Assert.Equal(10m, alpha.AvailableScore);
        Assert.Equal(85m, alpha.Percentage);
        Assert.Equal(2, alpha.JudgesCounted);
        Assert.Equal(2, alpha.Criteria[0].JudgesCounted);
    }

    [Fact]
    public void Calculate_ShouldOnlyCountPublishedJudgeScores()
    {
        // Judge 1 published (counts), Judge 2 not published yet (ignored)
        var scores = new List<QuestionScore>
        {
            Score(1, 1, 9m, 10m, judgeId: 1),
            Score(1, 1, 5m, 10m, judgeId: 2)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(90m, alpha.Percentage);
        Assert.Equal(1, alpha.JudgesCounted);
    }

    [Fact]
    public void Calculate_ShouldCountQuestionSkippedByOneJudgeButNotTheOther()
    {
        // Judge 2 skips -> excluded; judge 1's score counts
        var scores = new List<QuestionScore>
        {
            Score(1, 1, 10m, 10m, judgeId: 1),
            Score(1, 1, 0m, 10m, skipped: true, judgeId: 2)
        };
        var pubs = new List<CriterionPublication> { Pub(1, 1), Pub(1, 2) };

        var standings = StandingsCalculator.Calculate(scores, Teams, pubs);
        var alpha = standings.Single(row => row.TeamId == 1);

        Assert.Equal(10m, alpha.AchievedScore);
        Assert.Equal(10m, alpha.AvailableScore);
        Assert.Equal(100m, alpha.Percentage);
        Assert.Equal(1, alpha.JudgesCounted);
    }

    [Fact]
    public void Calculate_Should_ReturnNoRows_WhenThereAreNoTeams()
    {
        var standings = StandingsCalculator.Calculate(
            new List<QuestionScore>(),
            new List<Team>(),
            new List<CriterionPublication>());

        Assert.Empty(standings);
    }
}
