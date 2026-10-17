using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class JudgingController : ControllerBase
{
    private readonly JudgingService _judgingService;

    public JudgingController(JudgingService judgingService)
    {
        _judgingService = judgingService;
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

    [HttpPost("scores")]
    public async Task<ActionResult> SubmitScores([FromBody] SubmitScoresRequest request)
    {
        var scores = request.Scores.Select(item => (item.CriterionId, item.Score, item.Comment)).ToList();
        var result = await _judgingService.SubmitScoresAsync(request.TeamId, request.JudgeId, scores);
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
}

public sealed record SubmitScoresRequest(int TeamId, int JudgeId, List<JudgingScoreInput> Scores);
public sealed record JudgingScoreInput(int CriterionId, decimal Score, string? Comment);
