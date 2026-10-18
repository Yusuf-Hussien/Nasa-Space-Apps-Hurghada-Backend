using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Tests;

public class StandingsCalculatorTests
{
    [Fact]
    public void Calculate_ShouldRankTeamsByTotalScore_AndKeepTiedTeamsTogether()
    {
        var scores = new List<JudgeScore>
        {
            new() { TeamId = 1, Score = 9m },
            new() { TeamId = 1, Score = 8m },
            new() { TeamId = 2, Score = 9m },
            new() { TeamId = 2, Score = 8m },
            new() { TeamId = 3, Score = 5m }
        };

        var standings = StandingsCalculator.Calculate(scores, new[]
        {
            new Team { TeamId = 1, Name = "Alpha" },
            new Team { TeamId = 2, Name = "Beta" },
            new Team { TeamId = 3, Name = "Gamma" }
        });

        Assert.Equal(1, standings[0].Rank);
        Assert.Equal(1, standings[1].Rank);
        Assert.Equal(3, standings[2].Rank);
        Assert.Equal("Alpha", standings[0].TeamName);
        Assert.Equal("Beta", standings[1].TeamName);
        Assert.True(standings[0].IsTied);
        Assert.True(standings[1].IsTied);
    }

    [Fact]
    public void Calculate_Should_IncludeUnscoredTeams_WithNoRank()
    {
        var standings = StandingsCalculator.Calculate(
            new List<JudgeScore> { new() { TeamId = 1, Score = 10m } },
            new[]
            {
                new Team { TeamId = 1, Name = "Scored" },
                new Team { TeamId = 2, Name = "Unscored" }
            });

        Assert.Equal(2, standings.Count);

        var scored = standings.Single(row => row.TeamId == 1);
        Assert.Equal(1, scored.Rank);
        Assert.Equal(10m, scored.TotalScore);

        var unscored = standings.Single(row => row.TeamId == 2);
        Assert.Null(unscored.Rank);
        Assert.False(unscored.IsTied);
        Assert.Equal(0m, unscored.TotalScore);
    }

    [Fact]
    public void Calculate_Should_ReturnNoRows_WhenThereAreNoTeams()
    {
        var standings = StandingsCalculator.Calculate(
            new List<JudgeScore>(),
            new List<Team>());

        Assert.Empty(standings);
    }
}
