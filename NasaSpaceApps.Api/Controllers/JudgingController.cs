using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public JudgingController(JudgingService judgingService, ICurrentUser currentUser)
    {
        _judgingService = judgingService;
        _currentUser = currentUser;
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

    /// <summary>
    /// Submit scores. The judgeId is taken from the authenticated judge's token —
    /// it is NOT accepted from the request body.
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

        return Ok(result.Value!.Select(score => new
        {
            scoreId = score.ScoreId,
            teamId = score.TeamId,
            criterionId = score.CriterionId,
            score = score.Score,
            comment = score.Comment
        }));
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
