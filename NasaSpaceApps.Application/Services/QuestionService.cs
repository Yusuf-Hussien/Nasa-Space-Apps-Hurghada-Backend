using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Application.Services;

public sealed class QuestionService
{
    private readonly INasaSpaceAppsRepository _repository;

    public QuestionService(INasaSpaceAppsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<Question>>> GetVisibleQuestionsAsync(int? challengeId, ICurrentUser user)
    {
        // Admin / CoreTeam / Judge see all questions (with correct answers)
        if (user.IsStaff)
        {
            var all = await _repository.GetAllQuestionsAsync(challengeId);
            return Result<List<Question>>.Success(all);
        }

        // Participant (or anonymous) see only published, active questions
        var published = await _repository.GetPublishedQuestionsAsync(challengeId);
        return Result<List<Question>>.Success(published);
    }

    public async Task<Result<Question>> GetQuestionByIdAsync(int id, ICurrentUser user)
    {
        var question = await _repository.GetQuestionByIdAsync(id);
        if (question is null || !question.IsActive)
        {
            return Result<Question>.Failure("question.not_found", 404, "Question not found.");
        }

        // Participants can only see published questions
        if (!user.IsStaff && !question.IsPublished)
        {
            return Result<Question>.Failure("question.not_found", 404, "Question not found.");
        }

        return Result<Question>.Success(question);
    }

    public async Task<Result<Question>> CreateAsync(QuestionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Text))
        {
            return Result<Question>.Failure("question.invalid", 400, "Question text is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Type))
        {
            return Result<Question>.Failure("question.invalid", 400, "Question type is required.");
        }

        if (input.Points <= 0)
        {
            return Result<Question>.Failure("question.invalid", 400, "Points must be a positive integer.");
        }

        var challenge = await _repository.GetChallengeByIdAsync(input.ChallengeId);
        if (challenge is null)
        {
            return Result<Question>.Failure("challenge.not_found", 404, "Challenge not found.");
        }

        var question = new Question
        {
            ChallengeId = input.ChallengeId,
            Type = input.Type.Trim(),
            Text = input.Text.Trim(),
            Options = input.Options,
            CorrectAnswer = string.IsNullOrWhiteSpace(input.CorrectAnswer) ? null : input.CorrectAnswer.Trim(),
            Points = input.Points,
            IsPublished = false,
            PublishedAt = null,
            IsActive = true
        };

        var created = await _repository.AddQuestionAsync(question);
        return Result<Question>.Success(created);
    }

    public async Task<Result<Question>> UpdateAsync(int id, QuestionInput input)
    {
        var question = await _repository.GetQuestionByIdAsync(id);
        if (question is null || !question.IsActive)
        {
            return Result<Question>.Failure("question.not_found", 404, "Question not found.");
        }

        if (string.IsNullOrWhiteSpace(input.Text))
        {
            return Result<Question>.Failure("question.invalid", 400, "Question text is required.");
        }

        if (input.Points <= 0)
        {
            return Result<Question>.Failure("question.invalid", 400, "Points must be a positive integer.");
        }

        question.ChallengeId = input.ChallengeId;
        question.Type = input.Type.Trim();
        question.Text = input.Text.Trim();
        question.Options = input.Options;
        question.CorrectAnswer = string.IsNullOrWhiteSpace(input.CorrectAnswer) ? null : input.CorrectAnswer.Trim();
        question.Points = input.Points;

        await _repository.UpdateQuestionAsync(question);
        return Result<Question>.Success(question);
    }

    public async Task<Result<Question>> PublishAsync(int id, bool publish)
    {
        var question = await _repository.GetQuestionByIdAsync(id);
        if (question is null || !question.IsActive)
        {
            return Result<Question>.Failure("question.not_found", 404, "Question not found.");
        }

        question.IsPublished = publish;
        question.PublishedAt = publish ? DateTime.UtcNow : null;

        await _repository.UpdateQuestionAsync(question);
        return Result<Question>.Success(question);
    }

    public async Task<Result<Question>> SoftDeleteAsync(int id)
    {
        var question = await _repository.GetQuestionByIdAsync(id);
        if (question is null)
        {
            return Result<Question>.Failure("question.not_found", 404, "Question not found.");
        }

        question.IsActive = false;
        question.IsPublished = false;
        question.PublishedAt = null;

        await _repository.UpdateQuestionAsync(question);
        return Result<Question>.Success(question);
    }
}
