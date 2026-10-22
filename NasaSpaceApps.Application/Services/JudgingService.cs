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

    public async Task<Result<List<JudgingCriterion>>> GetCriteriaAsync(int? challengeId = null)
    {
        return Result<List<JudgingCriterion>>.Success(
            await _repository.GetJudgingCriteriaAsync(challengeId));
    }

    // ---- Criteria CRUD ----

    public async Task<Result<JudgingCriterion>> CreateCriterionAsync(JudgingCriterionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Result<JudgingCriterion>.Failure("criterion.invalid", 400, "Criterion name is required.");
        }

        if (input.MaxScore <= 0m)
        {
            return Result<JudgingCriterion>.Failure("criterion.invalid", 400, "Max score must be positive.");
        }

        if (input.ChallengeId.HasValue)
        {
            var challenge = await _repository.GetChallengeByIdAsync(input.ChallengeId.Value);
            if (challenge is null)
            {
                return Result<JudgingCriterion>.Failure("challenge.not_found", 404, "Challenge not found.");
            }
        }

        var criterion = new JudgingCriterion
        {
            ChallengeId = input.ChallengeId,
            Name = input.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            MaxScore = input.MaxScore,
            Weight = input.Weight,
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

        if (input.MaxScore <= 0m)
        {
            return Result<JudgingCriterion>.Failure("criterion.invalid", 400, "Max score must be positive.");
        }

        if (input.ChallengeId.HasValue)
        {
            var challenge = await _repository.GetChallengeByIdAsync(input.ChallengeId.Value);
            if (challenge is null)
            {
                return Result<JudgingCriterion>.Failure("challenge.not_found", 404, "Challenge not found.");
            }
        }

        criterion.ChallengeId = input.ChallengeId;
        criterion.Name = input.Name.Trim();
        criterion.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        criterion.MaxScore = input.MaxScore;
        criterion.Weight = input.Weight;

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

    // ---- Per-question scoring ----

    public async Task<Result<List<QuestionScore>>> SubmitScoresAsync(
        int teamId,
        int judgeId,
        List<QuestionScoreInput> scores)
    {
        var team = await _repository.GetTeamByIdAsync(teamId);
        if (team is null)
        {
            return Result<List<QuestionScore>>.Failure("team.not_found", 404, "The team does not exist.");
        }

        if (scores is null || scores.Count == 0)
        {
            return Result<List<QuestionScore>>.Failure("judging.no_scores", 400, "At least one question score is required.");
        }

        var existingScores = await _repository.GetQuestionScoresAsync(teamId: teamId);
        var persisted = new List<QuestionScore>();

        foreach (var item in scores)
        {
            var criterion = await _repository.GetJudgingCriterionByIdAsync(item.CriterionId);
            if (criterion is null)
            {
                return Result<List<QuestionScore>>.Failure("criterion.not_found", 404, $"Criterion {item.CriterionId} was not found.");
            }

            decimal maxScore;
            string? name = item.Name;

            if (item.CriterionQuestionId.HasValue)
            {
                var catalogueQuestion = await _repository.GetCriterionQuestionByIdAsync(item.CriterionQuestionId.Value);
                if (catalogueQuestion is null || catalogueQuestion.CriterionId != item.CriterionId)
                {
                    return Result<List<QuestionScore>>.Failure(
                        "criterion_question.not_found", 404,
                        $"Question {item.CriterionQuestionId} was not found on criterion {item.CriterionId}.");
                }

                name = catalogueQuestion.Text;
                maxScore = item.MaxScore ?? criterion.MaxScore;
            }
            else
            {
                maxScore = item.MaxScore ?? criterion.MaxScore;
            }

            if (maxScore <= 0m)
            {
                return Result<List<QuestionScore>>.Failure("score.invalid", 400, "Max score must be positive.");
            }

            if (!item.Skipped && (item.Score < 0m || item.Score > maxScore))
            {
                return Result<List<QuestionScore>>.Failure(
                    "score.invalid", 400,
                    $"Score '{name ?? item.CriterionQuestionId?.ToString()}' is outside the allowed range (0-{maxScore}).");
            }

            var existing = QuestionScoreMatch.FindExisting(existingScores, teamId, item.CriterionId, item.CriterionQuestionId, judgeId, name);
            if (existing is not null)
            {
                existing.CriterionQuestionId = item.CriterionQuestionId;
                existing.QuestionName = name;
                existing.MaxScore = maxScore;
                existing.Score = item.Skipped ? 0m : item.Score;
                existing.Skipped = item.Skipped;
                existing.Comment = item.Comment;
                existing.Timestamp = DateTime.UtcNow;
                await _repository.UpdateQuestionScoreAsync(existing);
                persisted.Add(existing);
                continue;
            }

            var score = new QuestionScore
            {
                TeamId = teamId,
                CriterionId = item.CriterionId,
                CriterionQuestionId = item.CriterionQuestionId,
                QuestionName = name,
                JudgeId = judgeId,
                MaxScore = maxScore,
                Score = item.Skipped ? 0m : item.Score,
                Skipped = item.Skipped,
                Comment = item.Comment,
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddQuestionScoreAsync(score);
            existingScores.Add(score);
            persisted.Add(score);
        }

        return Result<List<QuestionScore>>.Success(persisted);
    }

    /// <summary>
    /// Public scoreboard. Ranking by achieved percentage (not raw score); only
    /// published-by-judge criteria counts are included and averaged across judges.
    /// </summary>
    public async Task<Result<List<TeamStanding>>> GetStandingsAsync()
    {
        var scores = await _repository.GetAllQuestionScoresAsync();
        var teams = await _repository.GetTeamsAsync();
        var publications = await _repository.GetCriterionPublicationsAsync();
        var standings = StandingsCalculator.Calculate(scores, teams, publications);
        return Result<List<TeamStanding>>.Success(standings);
    }

    public async Task<Result<List<QuestionScore>>> GetScoresForTeamAsync(int teamId)
    {
        var scores = await _repository.GetQuestionScoresAsync(teamId);
        return Result<List<QuestionScore>>.Success(scores);
    }

    public async Task<Result<List<CriterionPublication>>> GetPublicationsAsync(int? criterionId = null, int? judgeId = null)
    {
        return Result<List<CriterionPublication>>.Success(
            await _repository.GetCriterionPublicationsAsync(criterionId, judgeId));
    }
}
