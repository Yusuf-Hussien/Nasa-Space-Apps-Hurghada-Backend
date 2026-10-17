using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;
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

    /// <summary>Side trophy leaderboard — public so spectators can view it.</summary>
    [AllowAnonymous]
    [HttpGet("leaderboard")]
    public async Task<ActionResult> GetLeaderboard()
    {
        var result = await _gameService.GetLeaderboardAsync();
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(row => new
        {
            rank = row.Rank,
            teamId = row.TeamId,
            teamName = row.TeamName,
            correctCount = row.CorrectCount,
            totalPoints = row.TotalPoints,
            isTied = row.IsTied
        }));
    }

    /// <summary>Submit an answer to a published question (Participant only, one per team).</summary>
    [Authorize(Policy = "ParticipantOnly")]
    [HttpPost("questions/{questionId:int}/answer")]
    public async Task<ActionResult> SubmitAnswer(int questionId, [FromBody] AnswerInput input)
    {
        var result = await _gameService.SubmitAnswerAsync(questionId, input.Answer, _currentUser);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new
        {
            correct = result.Value!.Correct,
            points = result.Value.Points,
            submittedAt = result.Value.SubmittedAt
        });
    }
}
