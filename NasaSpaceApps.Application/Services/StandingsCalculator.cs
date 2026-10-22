using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class TeamStanding
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int? Rank { get; set; }
    public bool IsTied { get; set; }

    /// <summary>Sum of scores awarded for published criteria (skipped questions excluded).</summary>
    public decimal AchievedScore { get; set; }

    /// <summary>Sum of max scores available across published criteria (skipped questions excluded).</summary>
    public decimal AvailableScore { get; set; }

    /// <summary>AchievedScore / AvailableScore * 100 — the ranking key. 0 when nothing available.</summary>
    public decimal Percentage { get; set; }

    /// <summary>Per-criterion totals for published criteria.</summary>
    public List<TeamStandingCriterionTotal> Criteria { get; set; } = new();

    /// <summary>
    /// How many distinct judges contributed to this team's standing. Fewer than
    /// the full panel means the result is based on partial judging.
    /// </summary>
    public int JudgesCounted { get; set; }
}

/// <summary>A single published criterion's contribution to a team's total.</summary>
public sealed record TeamStandingCriterionTotal(
    int CriterionId,
    string CriterionName,
    decimal AchievedScore,
    decimal AvailableScore,
    int JudgesCounted);

public static class StandingsCalculator
{
    /// <summary>
    /// Builds the public scoreboard from per-question scores.
    /// <para>
    /// A score only counts once its judge has <b>published</b> that criterion, so
    /// every judge can submit and publish a criterion independently. Within a
    /// criterion each judge's achieved/available totals are averaged, and the
    /// team's totals are the sum of its criterion totals. Skipped questions are
    /// excluded from the judge who skipped them, so a question skipped by one
    /// judge still counts for the others.
    /// </para>
    /// </summary>
    public static List<TeamStanding> Calculate(
        IEnumerable<QuestionScore> scores,
        IEnumerable<Team> teams,
        IEnumerable<CriterionPublication> publications)
    {
        var publishedKeys = publications
            .Select(publication => (publication.CriterionId, publication.JudgeId))
            .ToHashSet();

        var visible = scores
            .Where(score => !score.Skipped)
            .Where(score => publishedKeys.Contains((score.CriterionId, score.JudgeId)))
            .ToList();

        var byTeam = visible
            .GroupBy(score => score.TeamId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var rows = teams
            .Select(team =>
            {
                byTeam.TryGetValue(team.TeamId, out var teamScores);
                teamScores ??= [];

                var criteria = BuildCriterionTotals(teamScores);

                var achieved = criteria.Sum(item => item.AchievedScore);
                var available = criteria.Sum(item => item.AvailableScore);

                return new TeamStanding
                {
                    TeamId = team.TeamId,
                    TeamName = team.Name,
                    AchievedScore = achieved,
                    AvailableScore = available,
                    Percentage = available > 0m ? Math.Round(achieved / available * 100m, 2) : 0m,
                    Criteria = criteria,
                    JudgesCounted = teamScores.Select(score => score.JudgeId).Distinct().Count(),
                    Rank = null,
                    IsTied = false
                };
            })
            .OrderByDescending(row => row.Percentage)
            .ThenBy(row => row.TeamName)
            .ToList();

        if (rows.Count == 0)
        {
            return rows;
        }

        var previousPercentage = rows[0].Percentage;
        var currentRank = 1;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            if (row.AvailableScore <= 0m)
            {
                row.Rank = null;
                row.IsTied = false;
                continue;
            }

            if (i > 0 && row.Percentage == previousPercentage)
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

            previousPercentage = row.Percentage;
        }

        return rows;
    }

    /// <summary>
    /// Builds one entry per criterion for a team, averaging the judges who
    /// published that criterion and scored this team.
    /// </summary>
    private static List<TeamStandingCriterionTotal> BuildCriterionTotals(List<QuestionScore> teamScores) =>
        teamScores
            .GroupBy(score => score.CriterionId)
            .Select(group =>
            {
                // One judge may have scored several questions of the same criterion;
                // their totals are summed first, then averaged across judges.
                var perJudge = group
                    .GroupBy(score => score.JudgeId)
                    .Select(judgeGroup => new
                    {
                        JudgeId = judgeGroup.Key,
                        Achieved = judgeGroup.Sum(item => item.Score),
                        Available = judgeGroup.Sum(item => item.MaxScore)
                    })
                    .Where(judge => judge.Available > 0m)
                    .ToList();

                return new TeamStandingCriterionTotal(
                    CriterionId: group.Key,
                    CriterionName: group.First().Criterion?.Name ?? $"Criterion {group.Key}",
                    AchievedScore: Round(perJudge.Count == 0 ? 0m : perJudge.Average(judge => judge.Achieved)),
                    AvailableScore: Round(perJudge.Count == 0 ? 0m : perJudge.Average(judge => judge.Available)),
                    JudgesCounted: perJudge.Count);
            })
            .OrderBy(item => item.CriterionId)
            .ToList();

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
