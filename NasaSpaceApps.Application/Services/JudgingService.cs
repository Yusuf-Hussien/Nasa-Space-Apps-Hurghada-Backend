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
        return Result<List<JudgingCriterion>>.Success(
            await _repository.GetJudgingCriteriaAsync());
    }

    // ---- Criteria CRUD ----

    public async Task<Result<JudgingCriterion>> CreateCriterionAsync(JudgingCriterionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Result<JudgingCriterion>.Failure("criterion.invalid", 400, "Criterion name is required.");
        }

        var criterion = new JudgingCriterion
        {
            Name = input.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            MaxScore = JudgingRules.MaxScore,
            Weight = JudgingRules.Weight,
            IsPublished = false,
            PublishedAt = null
        };

        var created = await _repository.AddJudgingCriterionAsync(criterion);
        await SyncQuestionsAsync(created, input.Questions);
        return Result<JudgingCriterion>.Success(await _repository.GetJudgingCriterionByIdAsync(created.CriterionId) ?? created);
    }

    public async Task<Result<JudgingCriterion>> UpdateCriterionAsync(int criterionId, JudgingCriterionInput input)
    {
        var criterion = await _repository.GetJudgingCriterionByIdAsync(criterionId);
        if (criterion is null)
        {
            return Result<JudgingCriterion>.Failure("criterion.not_found", 404, "Criterion not found.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Result<JudgingCriterion>.Failure("criterion.invalid", 400, "Criterion name is required.");
        }

        criterion.Name = input.Name.Trim();
        criterion.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        criterion.MaxScore = JudgingRules.MaxScore;
        criterion.Weight = JudgingRules.Weight;

        await _repository.UpdateJudgingCriterionAsync(criterion);

        if (input.Questions is not null)
        {
            foreach (var existing in criterion.Questions.ToList())
            {
                await _repository.DeleteCriterionQuestionAsync(existing);
            }

            criterion.Questions.Clear();
            await SyncQuestionsAsync(criterion, input.Questions);
        }

        return Result<JudgingCriterion>.Success(
            await _repository.GetJudgingCriterionByIdAsync(criterionId) ?? criterion);
    }

    public async Task<Result<JudgingCriterion>> DeleteCriterionAsync(int criterionId)
    {
        var criterion = await _repository.GetJudgingCriterionByIdAsync(criterionId);
        if (criterion is null)
        {
            return Result<JudgingCriterion>.Failure("criterion.not_found", 404, "Criterion not found.");
        }

        foreach (var score in await _repository.GetQuestionScoresAsync(criterionId: criterionId))
        {
            await _repository.DeleteQuestionScoreAsync(score);
        }

        foreach (var score in await _repository.GetJudgeScoresForCriterionAsync(criterionId))
        {
            await _repository.DeleteJudgeScoreAsync(score);
        }

        foreach (var publication in await _repository.GetCriterionPublicationsAsync(criterionId: criterionId))
        {
            await _repository.DeleteCriterionPublicationAsync(publication);
        }

        foreach (var question in await _repository.GetCriterionQuestionsAsync(criterionId, activeOnly: false))
        {
            await _repository.DeleteCriterionQuestionAsync(question);
        }

        await _repository.DeleteJudgingCriterionAsync(criterion);
        return Result<JudgingCriterion>.Success(criterion);
    }

    /// <summary>
    /// Publish or unpublish a criterion for specific judges.
    /// A judge publishes their own scores; an Admin may target one judge or
    /// force-publish every judge who has scores on the criterion.
    /// </summary>
    public async Task<Result<JudgingCriterion>> PublishCriterionAsync(
        int criterionId,
        bool publish,
        string? callerRole,
        int? callerJudgeId,
        int? targetJudgeId = null)
    {
        if (!CriterionPublicationPolicy.CanPublish(callerRole))
        {
            return Result<JudgingCriterion>.Failure(
                "judging.not_allowed", 403, "Only a judge or an admin may publish criterion scores.");
        }

        var criterion = await _repository.GetJudgingCriterionByIdAsync(criterionId);
        if (criterion is null)
        {
            return Result<JudgingCriterion>.Failure("criterion.not_found", 404, "Criterion not found.");
        }

        var judgesWithScores = await _repository.GetJudgeIdsWithScoresForCriterionAsync(criterionId);
        var targets = CriterionPublicationPolicy.ResolveTargets(callerRole, callerJudgeId, targetJudgeId, judgesWithScores);

        if (targets.Count == 0)
        {
            if (string.Equals(callerRole, CriterionPublicationPolicy.JudgeRole, StringComparison.OrdinalIgnoreCase))
            {
                return Result<JudgingCriterion>.Failure(
                    "judging.no_scores", 400, "You have no scores to publish for this criterion.");
            }

            return Result<JudgingCriterion>.Failure(
                "judging.no_scores", 400, "No judges have scores for this criterion yet.");
        }

        if (publish)
        {
            foreach (var judgeId in targets)
            {
                var existing = await _repository.GetCriterionPublicationAsync(criterionId, judgeId);
                if (existing is null)
                {
                    await _repository.AddCriterionPublicationAsync(new CriterionPublication
                    {
                        CriterionId = criterionId,
                        JudgeId = judgeId,
                        PublishedAt = DateTime.UtcNow
                    });
                }
            }
        }
        else
        {
            foreach (var judgeId in targets)
            {
                var existing = await _repository.GetCriterionPublicationAsync(criterionId, judgeId);
                if (existing is not null)
                {
                    await _repository.DeleteCriterionPublicationAsync(existing);
                }
            }
        }

        // Keep the legacy flag in sync for compatibility (true if any judge has published).
        var allPublications = await _repository.GetCriterionPublicationsAsync(criterionId: criterionId);
        criterion.IsPublished = allPublications.Count > 0;
        criterion.PublishedAt = allPublications.Count > 0
            ? allPublications.Max(publication => publication.PublishedAt)
            : null;
        await _repository.UpdateJudgingCriterionAsync(criterion);

        return Result<JudgingCriterion>.Success(
            await _repository.GetJudgingCriterionByIdAsync(criterionId) ?? criterion);
    }

    private async Task SyncQuestionsAsync(JudgingCriterion criterion, List<CriterionQuestionInput>? questions)
    {
        if (questions is null)
        {
            return;
        }

        foreach (var question in questions.Where(q => !string.IsNullOrWhiteSpace(q.Text)))
        {
            await _repository.AddCriterionQuestionAsync(new CriterionQuestion
            {
                CriterionId = criterion.CriterionId,
                Text = question.Text.Trim(),
                DisplayOrder = question.DisplayOrder,
                IsActive = true
            });
        }
    }

    // ---- Criterion-level scoring ----

    public async Task<Result<List<JudgeScore>>> SubmitScoresAsync(
        int teamId,
        int judgeId,
        List<CriterionScoreInput> scores)
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

        if (scores.Select(item => item.CriterionId).Distinct().Count() != scores.Count)
        {
            return Result<List<JudgeScore>>.Failure(
                "score.duplicate", 400, "Each criterion may only be scored once per submission.");
        }

        var criteria = new Dictionary<int, JudgingCriterion>();

        foreach (var item in scores)
        {
            if (!criteria.TryGetValue(item.CriterionId, out var criterion))
            {
                var found = await _repository.GetJudgingCriterionByIdAsync(item.CriterionId);
                if (found is null)
                {
                    return Result<List<JudgeScore>>.Failure(
                        "criterion.not_found", 404, $"Criterion {item.CriterionId} was not found.");
                }

                criterion = found;
                criteria[item.CriterionId] = criterion;
            }

            if (item.Score < JudgingRules.MinimumScore || item.Score > criterion.MaxScore)
            {
                return Result<List<JudgeScore>>.Failure(
                    "score.invalid", 400,
                    $"{criterion.Name} score must be between {JudgingRules.MinimumScore:0.0} and {criterion.MaxScore:0.0}.");
            }
        }

        var persisted = new List<JudgeScore>();

        foreach (var item in scores)
        {
            var criterion = criteria[item.CriterionId];
            var existing = await _repository.GetJudgeScoreAsync(teamId, judgeId, item.CriterionId);
            if (existing is not null)
            {
                existing.MaxScore = criterion.MaxScore;
                existing.Score = item.Score;
                existing.Comment = item.Comment;
                existing.Timestamp = DateTime.UtcNow;
                await _repository.UpdateJudgeScoreAsync(existing);
                persisted.Add(existing);
                continue;
            }

            var score = new JudgeScore
            {
                TeamId = teamId,
                JudgeId = judgeId,
                CriterionId = item.CriterionId,
                MaxScore = criterion.MaxScore,
                Score = item.Score,
                Comment = item.Comment,
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddJudgeScoreAsync(score);
            persisted.Add(score);
        }

        return Result<List<JudgeScore>>.Success(persisted);
    }

    /// <summary>
    /// Public scoreboard. Ranking by achieved percentage (not raw score); only
    /// scores published by their judge are included and averaged across judges.
    /// </summary>
    public async Task<Result<List<TeamStanding>>> GetStandingsAsync()
    {
        var scores = await _repository.GetAllScoresAsync();
        var teams = await _repository.GetTeamsAsync();
        var publications = await _repository.GetCriterionPublicationsAsync();
        var standings = StandingsCalculator.Calculate(scores, teams, publications);
        return Result<List<TeamStanding>>.Success(standings);
    }

    public async Task<Result<List<JudgeScore>>> GetScoresForTeamAsync(int teamId)
    {
        var scores = await _repository.GetScoresForTeamAsync(teamId);
        return Result<List<JudgeScore>>.Success(scores);
    }

    public async Task<Result<List<CriterionPublication>>> GetPublicationsAsync(int? criterionId = null, int? judgeId = null)
    {
        return Result<List<CriterionPublication>>.Success(
            await _repository.GetCriterionPublicationsAsync(criterionId, judgeId));
    }
}
