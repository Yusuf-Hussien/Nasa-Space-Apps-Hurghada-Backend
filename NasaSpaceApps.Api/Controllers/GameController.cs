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

    /// <summary>Current competition state. Public so clients can show the waiting screen.</summary>
    [AllowAnonymous]
    [HttpGet("state")]
    public async Task<ActionResult<GameStateResponse>> GetState()
    {
        var result = await _gameService.GetStateAsync();
        return result.ToHttpResult(this, GameStateResponse.From);
    }

    /// <summary>Start the competition so participants can see released questions.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("start")]
    public async Task<ActionResult<GameStateResponse>> Start()
    {
        var result = await _gameService.StartAsync();
        return result.ToHttpResult(this, GameStateResponse.From);
    }

    /// <summary>Stop the competition. Participants immediately lose access to questions.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("stop")]
    public async Task<ActionResult<GameStateResponse>> Stop()
    {
        var result = await _gameService.StopAsync();
        return result.ToHttpResult(this, GameStateResponse.From);
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
