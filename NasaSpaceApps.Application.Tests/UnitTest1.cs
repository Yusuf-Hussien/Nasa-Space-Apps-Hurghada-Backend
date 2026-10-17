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
}
