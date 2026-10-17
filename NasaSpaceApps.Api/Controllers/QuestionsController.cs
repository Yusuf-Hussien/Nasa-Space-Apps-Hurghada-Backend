using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly QuestionService _questionService;
    private readonly ICurrentUser _currentUser;

    public QuestionsController(QuestionService questionService, ICurrentUser currentUser)
    {
        _questionService = questionService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Staff (Admin/CoreTeam/Judge) see all questions with correct answers.
    /// Participants see only published questions without correct answers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetQuestions([FromQuery] int? challengeId = null)
    {
        var result = await _questionService.GetVisibleQuestionsAsync(challengeId, _currentUser);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var showAnswers = _currentUser.IsStaff;

        return Ok(result.Value!.Select(q => new
        {
            questionId = q.QuestionId,
            challengeId = q.ChallengeId,
            type = q.Type,
            text = q.Text,
            options = q.Options,
            correctAnswer = showAnswers ? q.CorrectAnswer : null,
            isPublished = q.IsPublished,
            publishedAt = q.PublishedAt,
            points = q.Points,
            isActive = q.IsActive
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetQuestion(int id)
    {
        var result = await _questionService.GetQuestionByIdAsync(id, _currentUser);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var q = result.Value!;
        var showAnswers = _currentUser.IsStaff;

        return Ok(new
        {
            questionId = q.QuestionId,
            challengeId = q.ChallengeId,
            type = q.Type,
            text = q.Text,
            options = q.Options,
            correctAnswer = showAnswers ? q.CorrectAnswer : null,
            isPublished = q.IsPublished,
            publishedAt = q.PublishedAt,
            points = q.Points,
            isActive = q.IsActive
        });
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult> CreateQuestion([FromBody] QuestionInputDto input)
    {
        var result = await _questionService.CreateAsync(new QuestionInput(
            input.ChallengeId, input.Type, input.Text, input.Options, input.CorrectAnswer, input.Points));

        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateQuestion(int id, [FromBody] QuestionInputDto input)
    {
        var result = await _questionService.UpdateAsync(id, new QuestionInput(
            input.ChallengeId, input.Type, input.Text, input.Options, input.CorrectAnswer, input.Points));

        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult> PublishQuestion(int id)
    {
        var result = await _questionService.PublishAsync(id, publish: true);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id:int}/unpublish")]
    public async Task<ActionResult> UnpublishQuestion(int id)
    {
        var result = await _questionService.PublishAsync(id, publish: false);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(ToDto(result.Value!, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteQuestion(int id)
    {
        var result = await _questionService.SoftDeleteAsync(id);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { questionId = result.Value!.QuestionId, isActive = false });
    }

    private static object ToDto(Domain.Entities.Question q, bool includeAnswer) => new
    {
        questionId = q.QuestionId,
        challengeId = q.ChallengeId,
        type = q.Type,
        text = q.Text,
        options = q.Options,
        correctAnswer = includeAnswer ? q.CorrectAnswer : null,
        isPublished = q.IsPublished,
        publishedAt = q.PublishedAt,
        points = q.Points,
        isActive = q.IsActive
    };
}
