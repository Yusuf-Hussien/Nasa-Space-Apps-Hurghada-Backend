using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Api.Hubs;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly QuestionService _questionService;
    private readonly GameService _gameService;
    private readonly ICurrentUser _currentUser;
    private readonly IHubContext<GameHub> _gameHub;

    public QuestionsController(
        QuestionService questionService,
        GameService gameService,
        ICurrentUser currentUser,
        IHubContext<GameHub> gameHub)
    {
        _questionService = questionService;
        _gameService = gameService;
        _currentUser = currentUser;
        _gameHub = gameHub;
    }

    /// <summary>
    /// Staff (Admin/CoreTeam/Judge) see all questions with correct answers.
    /// Participants see only published questions without correct answers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<QuestionResponse>>> GetQuestions([FromQuery] int? challengeId = null)
    {
        var result = await _questionService.GetVisibleQuestionsAsync(challengeId, _currentUser);
        var showAnswers = _currentUser.IsStaff;
        return result.ToHttpResult(
            this,
            questions => questions.Select(q => QuestionResponse.From(q, showAnswers)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionResponse>> GetQuestion(int id)
    {
        var result = await _questionService.GetQuestionByIdAsync(id, _currentUser);
        return result.ToHttpResult(this, q => QuestionResponse.From(q, _currentUser.IsStaff));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> CreateQuestion([FromBody] QuestionInput input)
    {
        var result = await _questionService.CreateAsync(input);
        return result.ToHttpResult(this, q => QuestionResponse.From(q, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<QuestionResponse>> UpdateQuestion(int id, [FromBody] QuestionInput input)
    {
        var result = await _questionService.UpdateAsync(id, input);
        if (result.IsSuccess && result.Value is not null)
        {
            await BroadcastQuestionAsync(GameHub.EventQuestionUpdated, result.Value);
        }

        return result.ToHttpResult(this, q => QuestionResponse.From(q, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<QuestionResponse>> PublishQuestion(int id)
    {
        var result = await _questionService.PublishAsync(id, publish: true);
        if (result.IsSuccess && result.Value is not null)
        {
            await BroadcastQuestionAsync(GameHub.EventQuestionReleased, result.Value);
        }

        return result.ToHttpResult(this, q => QuestionResponse.From(q, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id:int}/unpublish")]
    public async Task<ActionResult<QuestionResponse>> UnpublishQuestion(int id)
    {
        var result = await _questionService.PublishAsync(id, publish: false);
        if (result.IsSuccess)
        {
            await BroadcastQuestionRetractedAsync(id);
        }

        return result.ToHttpResult(this, q => QuestionResponse.From(q, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<DeletedResponse>> DeleteQuestion(int id)
    {
        var result = await _questionService.SoftDeleteAsync(id);
        if (result.IsSuccess)
        {
            await BroadcastQuestionRetractedAsync(id);
        }

        return result.ToHttpResult(this, _ => DeletedResponse.Of(id));
    }

    private async Task BroadcastQuestionAsync(string eventName, Question question)
    {
        await _gameHub.Clients.Group(GameHub.AdminsGroup)
            .SendAsync(eventName, QuestionResponse.From(question, includeAnswer: true));

        var state = await _gameService.GetStateAsync();
        if (state.IsSuccess && GameVisibilityPolicy.CanParticipantSee(state.Value, question))
        {
            await _gameHub.Clients.Group(GameHub.ParticipantsGroup)
                .SendAsync(eventName, QuestionResponse.From(question, includeAnswer: false));
        }
    }

    private async Task BroadcastQuestionRetractedAsync(int questionId)
    {
        var payload = new GameQuestionRetractedEvent(questionId);
        await Task.WhenAll(
            _gameHub.Clients.Group(GameHub.AdminsGroup)
                .SendAsync(GameHub.EventQuestionRetracted, payload),
            _gameHub.Clients.Group(GameHub.ParticipantsGroup)
                .SendAsync(GameHub.EventQuestionRetracted, payload));
    }
}
