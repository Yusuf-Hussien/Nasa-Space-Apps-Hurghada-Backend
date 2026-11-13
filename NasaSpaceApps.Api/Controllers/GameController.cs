using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Api.Hubs;
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
    private readonly IHubContext<GameHub> _gameHub;

    public GameController(
        GameService gameService,
        ICurrentUser currentUser,
        IHubContext<GameHub> gameHub)
    {
        _gameService = gameService;
        _currentUser = currentUser;
        _gameHub = gameHub;
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
        if (result.IsSuccess && result.Value is not null)
        {
            await BroadcastStateAsync(GameStateResponse.From(result.Value));
        }

        return result.ToHttpResult(this, GameStateResponse.From);
    }

    /// <summary>Stop the competition. Participants immediately lose access to questions.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("stop")]
    public async Task<ActionResult<GameStateResponse>> Stop()
    {
        var result = await _gameService.StopAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            await BroadcastStateAsync(GameStateResponse.From(result.Value));
        }

        return result.ToHttpResult(this, GameStateResponse.From);
    }

    /// <summary>Side trophy leaderboard. Visible to admins only.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardRow>>> GetLeaderboard()
    {
        var result = await _gameService.GetLeaderboardAsync();
        return result.ToHttpResult(this);
    }

    /// <summary>Submit an answer to a published question (Participant only, one per team).</summary>
    [Authorize(Policy = "ParticipantOnly")]
    [HttpPost("questions/{questionId:int}/answer")]
    public async Task<ActionResult<AnswerResponse>> SubmitAnswer(int questionId, [FromBody] AnswerInput input)
    {
        var result = await _gameService.SubmitAnswerAsync(questionId, input.Answer, _currentUser);
        if (result.IsSuccess && result.Value is not null)
        {
            await BroadcastAnswerAsync(questionId, input.Answer, result.Value);
        }

        return result.ToHttpResult(this, AnswerResponse.From);
    }

    private async Task BroadcastStateAsync(GameStateResponse state)
    {
        await Task.WhenAll(
            _gameHub.Clients.Group(GameHub.AdminsGroup)
                .SendAsync(GameHub.EventGameStateChanged, state),
            _gameHub.Clients.Group(GameHub.ParticipantsGroup)
                .SendAsync(GameHub.EventGameStateChanged, state));
    }

    private async Task BroadcastAnswerAsync(int questionId, string answer, AnswerResult result)
    {
        var teamId = _currentUser.TeamId ?? 0;
        var payload = new GameAnswerSubmittedEvent(
            QuestionId: questionId,
            TeamId: teamId,
            Answer: answer,
            Correct: result.Correct,
            Points: result.Points,
            SubmittedAt: result.SubmittedAt);

        await _gameHub.Clients.Group(GameHub.AdminsGroup)
            .SendAsync(GameHub.EventAnswerSubmitted, payload);

        var leaderboard = await _gameService.GetLeaderboardAsync();
        if (leaderboard.IsSuccess && leaderboard.Value is not null)
        {
            await _gameHub.Clients.Group(GameHub.AdminsGroup)
                .SendAsync(GameHub.EventLeaderboardUpdated, leaderboard.Value);
        }
    }
}
