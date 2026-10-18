using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class TeamStanding
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? Rank { get; set; }
    public bool IsTied { get; set; }
    public decimal TotalScore { get; set; }

    /// <summary>Per-criterion breakdown of how <see cref="TotalScore"/> was reached.</summary>
    public List<TeamStandingScore> Scores { get; set; } = new();
}

/// <summary>A single criterion's contribution to a team's total.</summary>
public sealed record TeamStandingScore(
    int CriterionId,
    string CriterionName,
    decimal Score,
    int JudgeId);

public static class StandingsCalculator
{
    public static List<TeamStanding> Calculate(IEnumerable<JudgeScore> scores, IEnumerable<Team> teams)
    {
        var teamMap = teams.ToDictionary(team => team.TeamId, team => team.Name);
        var totals = scores
            .GroupBy(score => score.TeamId)
            .Select(group => new { TeamId = group.Key, TotalScore = group.Sum(item => item.Score) })
            .ToDictionary(item => item.TeamId, item => item.TotalScore);

        var rows = teamMap
            .Select(team => new TeamStanding
            {
                TeamId = team.Key,
                TeamName = team.Value,
                TotalScore = totals.TryGetValue(team.Key, out var total) ? total : 0m,
                Rank = null,
                IsTied = false
            })
            .OrderByDescending(row => row.TotalScore)
            .ThenBy(row => row.TeamName)
            .ToList();

        if (rows.Count == 0)
        {
            return rows;
        }

        var previousTotal = rows[0].TotalScore;
        var currentRank = 1;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            if (row.TotalScore <= 0m)
            {
                row.Rank = null;
                row.IsTied = false;
                continue;
            }

            if (i > 0 && row.TotalScore == previousTotal)
            {
                row.Rank = currentRank;
                row.IsTied = true;
                rows[i - 1].IsTied = true;
            }
            else
            {
                currentRank = i + 1;
                row.Rank = currentRank;
                row.IsTied = false;
            }

            previousTotal = row.TotalScore;
        }

        return rows;
    }
}
