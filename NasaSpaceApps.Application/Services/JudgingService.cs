using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class JudgingService
{
    private readonly INasaSpaceAppsRepository _repository;

    public JudgingService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<JudgingCriterion>>> GetCriteriaAsync()
    {
        return Result<List<JudgingCriterion>>.Success(await _repository.GetJudgingCriteriaAsync());
    }

    public async Task<Result<List<JudgeScore>>> SubmitScoresAsync(
        int teamId,
        int judgeId,
        List<(int CriterionId, decimal Score, string? Comment)> scores)
    {
        var team = await _repository.GetTeamByIdAsync(teamId);
        if (team is null)
        {
            return Result<List<JudgeScore>>.Failure("team.not_found", 404, "The team does not exist.");
        }

        if (scores is null || scores.Count == 0)
        {
            return Result<List<JudgeScore>>.Failure("judging.no_scores", 400, "At least one criterion score is required.");
        }

        var persistedScores = new List<JudgeScore>();

        foreach (var item in scores)
        {
            var criterion = await _repository.GetJudgingCriterionByIdAsync(item.CriterionId);
            if (criterion is null)
            {
                return Result<List<JudgeScore>>.Failure("criterion.not_found", 404, $"Criterion {item.CriterionId} was not found.");
            }

            if (item.Score < 0m || item.Score > criterion.MaxScore)
            {
                return Result<List<JudgeScore>>.Failure("score.invalid", 400, $"Score for criterion {criterion.Name} is outside the allowed range (0-{criterion.MaxScore}).");
            }

            var score = new JudgeScore
            {
                TeamId = teamId,
                JudgeId = judgeId,
                CriterionId = item.CriterionId,
                Score = item.Score,
                Comment = item.Comment,
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddJudgeScoreAsync(score);
            persistedScores.Add(score);
        }

        return Result<List<JudgeScore>>.Success(persistedScores);
    }

    /// <summary>
    /// Single source of truth for the scoreboard. Ranks and ties come from
    /// <see cref="StandingsCalculator"/>; this only attaches the per-criterion breakdown.
    /// </summary>
    public async Task<Result<List<TeamStanding>>> GetStandingsAsync()
    {
        var scores = await _repository.GetAllScoresAsync();

        var teams = await _repository.GetTeamsAsync();
        var standings = StandingsCalculator.Calculate(scores, teams);

        var breakdown = scores
            .GroupBy(score => score.TeamId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(score => new TeamStandingScore(
                        score.CriterionId,
                        score.Criterion?.Name ?? $"Criterion {score.CriterionId}",
                        score.Score,
                        score.JudgeId))
                    .ToList());

        foreach (var row in standings)
        {
            if (breakdown.TryGetValue(row.TeamId, out var perCriterion))
            {
                row.Scores = perCriterion;
            }
        }

        return Result<List<TeamStanding>>.Success(standings);
    }

    public async Task<Result<List<JudgeScore>>> GetScoresForTeamAsync(int teamId)
    {
        var scores = await _repository.GetScoresForTeamAsync(teamId);
        return Result<List<JudgeScore>>.Success(scores);
    }
}
