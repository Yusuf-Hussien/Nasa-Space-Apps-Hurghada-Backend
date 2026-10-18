using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ICurrentUser _currentUser;

    public GameController(GameService gameService, ICurrentUser currentUser)
    {
        _gameService = gameService;
        _currentUser = currentUser;
    }

    /// <summary>Side trophy leaderboard. Public so spectators can view it.</summary>
    [AllowAnonymous]
    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardRow>>> GetLeaderboard()
    {
        var result = await _gameService.GetLeaderboardAsync();
        return result.ToHttpResult(this);
    }

    /// <summary>Submit an answer to a published question (Participant only, one per team).</summary>
    [Authorize(Policy = "ParticipantOnly")]
    [HttpPost("questions/{questionId:int}/answer")]
    public async Task<ActionResult<AnswerResult>> SubmitAnswer(int questionId, [FromBody] AnswerInput input)
    {
        var result = await _gameService.SubmitAnswerAsync(questionId, input.Answer, _currentUser);
        return result.ToHttpResult(this);
    }
}
