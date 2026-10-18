using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Api.Hubs;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class JudgingController : ControllerBase
{
    private readonly JudgingService _judgingService;
    private readonly ICurrentUser _currentUser;
    private readonly IHubContext<JudgingHub> _hubContext;

    public JudgingController(
        JudgingService judgingService,
        ICurrentUser currentUser,
        IHubContext<JudgingHub> hubContext)
    {
        _judgingService = judgingService;
        _currentUser = currentUser;
        _hubContext = hubContext;
    }

    [HttpGet("criteria")]
    public async Task<ActionResult<List<JudgingCriterionResponse>>> GetCriteria()
    {
        var result = await _judgingService.GetCriteriaAsync();
        return result.ToHttpResult(this, items => items.Select(JudgingCriterionResponse.From).ToList());
    }

    /// <summary>Public scoreboard. Single source of truth for standings.</summary>
    [AllowAnonymous]
    [HttpGet("standings")]
    public async Task<ActionResult<List<StandingsEntryResponse>>> GetStandings()
    {
        var result = await _judgingService.GetStandingsAsync();
        return result.ToHttpResult(this, rows => rows.Select(StandingsEntryResponse.From).ToList());
    }

    /// <summary>
    /// Submit scores. The judgeId is taken from the authenticated judge's token,
    /// it is NOT accepted from the request body. Broadcasts the updated standings to everyone via SignalR.
    /// </summary>
    [Authorize(Policy = "JudgeOnly")]
    [HttpPost("scores")]
    public async Task<ActionResult<List<JudgeScoreResponse>>> SubmitScores([FromBody] SubmitScoresRequest request)
    {
        var judgeId = _currentUser.JudgeId;
        if (!judgeId.HasValue)
        {
            return StatusCode(403, new { error = new { code = "judging.no_judge_link", message = "Your account is not linked to a judge profile." } });
        }

        var scores = request.Scores.Select(item => (item.CriterionId, item.Score, item.Comment)).ToList();
        var result = await _judgingService.SubmitScoresAsync(request.TeamId, judgeId.Value, scores);
        var submitted = result.IsSuccess
            ? result.Value!.Select(score => JudgeScoreResponse.From(score, includeJudge: false)).ToList()
            : null;

        var httpResult = result.ToHttpResult(this, _ => submitted!);
        if (submitted is null)
        {
            return httpResult;
        }

        await _hubContext.Clients.All.SendAsync(
            JudgingHub.EventScoresSubmitted,
            new { teamId = request.TeamId, judgeId, scores = submitted });

        await BroadcastStandingsAsync();
        return httpResult;
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpGet("scores")]
    public async Task<ActionResult<List<JudgeScoreResponse>>> GetScores([FromQuery] int teamId)
    {
        var result = await _judgingService.GetScoresForTeamAsync(teamId);
        return result.ToHttpResult(this, scores => scores.Select(s => JudgeScoreResponse.From(s)).ToList());
    }

    private async Task BroadcastStandingsAsync()
    {
        var standingsResult = await _judgingService.GetStandingsAsync();
        if (!standingsResult.IsSuccess)
        {
            return;
        }

        var standings = standingsResult.Value!.Select(StandingsEntryResponse.From).ToList();
        await _hubContext.Clients.All.SendAsync(JudgingHub.EventStandingsUpdated, standings);
    }
}
