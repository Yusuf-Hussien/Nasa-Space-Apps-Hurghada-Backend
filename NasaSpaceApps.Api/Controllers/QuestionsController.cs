using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
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
        return result.ToHttpResult(this, q => QuestionResponse.From(q, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id:int}/publish")]
    public async Task<ActionResult<QuestionResponse>> PublishQuestion(int id)
    {
        var result = await _questionService.PublishAsync(id, publish: true);
        return result.ToHttpResult(this, q => QuestionResponse.From(q, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id:int}/unpublish")]
    public async Task<ActionResult<QuestionResponse>> UnpublishQuestion(int id)
    {
        var result = await _questionService.PublishAsync(id, publish: false);
        return result.ToHttpResult(this, q => QuestionResponse.From(q, includeAnswer: true));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<DeletedResponse>> DeleteQuestion(int id)
    {
        var result = await _questionService.SoftDeleteAsync(id);
        return result.ToHttpResult(this, _ => DeletedResponse.Of(id));
    }
}
