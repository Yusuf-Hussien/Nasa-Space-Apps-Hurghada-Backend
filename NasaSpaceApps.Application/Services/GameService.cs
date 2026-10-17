using System.Text.Json;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class GameService
{
    private readonly INasaSpaceAppsRepository _repository;

    public GameService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AnswerResult>> SubmitAnswerAsync(int questionId, string answer, ICurrentUser user)
    {
        if (!user.TeamId.HasValue)
        {
            return Result<AnswerResult>.Failure("game.not_participant", 403, "Only participants can submit answers.");
        }

        var teamId = user.TeamId.Value;
        var question = await _repository.GetQuestionByIdAsync(questionId);

        if (question is null || !question.IsActive || !question.IsPublished)
        {
            return Result<AnswerResult>.Failure("question.not_found", 404, "Question not found or not published.");
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            return Result<AnswerResult>.Failure("answer.empty", 400, "Answer is required.");
        }

        var existing = await _repository.GetParticipationAsync(questionId, teamId);
        if (existing is not null)
        {
            return Result<AnswerResult>.Failure("answer.already_submitted", 409, "Your team has already answered this question.");
        }

        var correct = IsCorrect(question, answer.Trim());

        var participation = new GameParticipation
        {
            QuestionId = questionId,
            TeamId = teamId,
            Answer = answer.Trim(),
            Correct = correct,
            SubmittedAt = DateTime.UtcNow
        };

        await _repository.AddParticipationAsync(participation);

        return Result<AnswerResult>.Success(new AnswerResult(
            Correct: correct,
            Points: correct ? question.Points : 0,
            SubmittedAt: participation.SubmittedAt));
    }

    public async Task<Result<List<LeaderboardRow>>> GetLeaderboardAsync()
    {
        var totals = await _repository.GetTeamGameTotalsAsync();
        var teams = await _repository.GetTeamsAsync();

        var teamLookup = teams.ToDictionary(t => t.TeamId, t => t.Name);

        var rows = totals
            .Select(kvp => new
            {
                TeamId = kvp.Key,
                TeamName = teamLookup.GetValueOrDefault(kvp.Key, $"Team {kvp.Key}"),
                CorrectCount = kvp.Value.CorrectCount,
                TotalPoints = kvp.Value.TotalPoints
            })
            .OrderByDescending(r => r.TotalPoints)
            .ThenByDescending(r => r.CorrectCount)
            .ThenBy(r => r.TeamName)
            .ToList();

        var result = new List<LeaderboardRow>();
        var previous = (TotalPoints: -1, CorrectCount: -1);
        var currentRank = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var key = (row.TotalPoints, row.CorrectCount);

            if (key != previous)
            {
                currentRank = i + 1;
                previous = key;
            }

            var isTied = rows.Count(r => (r.TotalPoints, r.CorrectCount) == key) > 1;

            result.Add(new LeaderboardRow(
                Rank: currentRank,
                TeamId: row.TeamId,
                TeamName: row.TeamName,
                CorrectCount: row.CorrectCount,
                TotalPoints: row.TotalPoints,
                IsTied: isTied));
        }

        return Result<List<LeaderboardRow>>.Success(result);
    }

    private static bool IsCorrect(Question question, string answer)
    {
        if (string.IsNullOrWhiteSpace(question.CorrectAnswer))
        {
            return false;
        }

        // Free-text: case-insensitive trim compare
        if (string.IsNullOrWhiteSpace(question.Options))
        {
            return string.Equals(answer.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // Multiple-choice: CorrectAnswer can be the option text or the zero-based index
        if (int.TryParse(question.CorrectAnswer, out var correctIndex))
        {
            var options = ParseOptions(question.Options);
            if (correctIndex >= 0 && correctIndex < options.Count)
            {
                return string.Equals(answer.Trim(), options[correctIndex], StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        return string.Equals(answer.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> ParseOptions(string optionsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
