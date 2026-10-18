using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

    public JudgingController(JudgingService judgingService, ICurrentUser currentUser, IHubContext<JudgingHub> hubContext)
    {
        _judgingService = judgingService;
        _currentUser = currentUser;
        _hubContext = hubContext;
    }

    [HttpGet("criteria")]
    public async Task<ActionResult> GetCriteria()
    {
        var result = await _judgingService.GetCriteriaAsync();
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(item => new
        {
            criterionId = item.CriterionId,
            name = item.Name,
            description = item.Description,
            maxScore = item.MaxScore,
            weight = item.Weight
        }));
    }

    [AllowAnonymous]
    [HttpGet("standings")]
    public async Task<ActionResult> GetStandings()
    {
        var result = await _judgingService.GetStandingsAsync();
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select((entry, index) => new
        {
            rank = index + 1,
            teamId = entry.TeamId,
            teamName = entry.TeamName,
            totalScore = entry.TotalScore,
            scores = entry.Scores.Select(score => new
            {
                criterionId = score.CriterionId,
                criterionName = score.CriterionName,
                score = score.Score,
                judgeId = score.JudgeId
            })
        }));
    }

    /// <summary>
    /// Submit scores. The judgeId is taken from the authenticated judge's token —
    /// it is NOT accepted from the request body. Broadcasts the updated standings to everyone via SignalR.
    /// </summary>
    [Authorize(Policy = "JudgeOnly")]
    [HttpPost("scores")]
    public async Task<ActionResult> SubmitScores([FromBody] SubmitScoresRequest request)
    {
        var judgeId = _currentUser.JudgeId;
        if (!judgeId.HasValue)
        {
            return StatusCode(403, new { code = "judging.no_judge_link", message = "Your account is not linked to a judge profile." });
        }

        var scores = request.Scores.Select(item => (item.CriterionId, item.Score, item.Comment)).ToList();
        var result = await _judgingService.SubmitScoresAsync(request.TeamId, judgeId.Value, scores);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var submitted = result.Value!.Select(score => new
        {
            scoreId = score.ScoreId,
            teamId = score.TeamId,
            criterionId = score.CriterionId,
            score = score.Score,
            comment = score.Comment
        }).ToList();
        await _hubContext.Clients.All.SendAsync("ScoresSubmitted", new { teamId = request.TeamId, judgeId, scores = submitted });

        var standingsResult = await _judgingService.GetStandingsAsync();
        if (standingsResult.IsSuccess)
        {
            var standings = standingsResult.Value!.Select((entry, index) => new
            {
                rank = index + 1,
                teamId = entry.TeamId,
                teamName = entry.TeamName,
                totalScore = entry.TotalScore,
                scores = entry.Scores.Select(score => new
                {
                    criterionId = score.CriterionId,
                    criterionName = score.CriterionName,
                    score = score.Score,
                    judgeId = score.JudgeId
                })
            });
            await _hubContext.Clients.All.SendAsync("StandingsUpdated", standings);
        }

        return Ok(submitted);
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpGet("scores")]
    public async Task<ActionResult> GetScores([FromQuery] int teamId)
    {
        var result = await _judgingService.GetScoresForTeamAsync(teamId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(score => new
        {
            scoreId = score.ScoreId,
            teamId = score.TeamId,
            judgeId = score.JudgeId,
            criterionId = score.CriterionId,
            criterionName = score.Criterion?.Name,
            score = score.Score,
            comment = score.Comment,
            timestamp = score.Timestamp
        }));
    }
}
