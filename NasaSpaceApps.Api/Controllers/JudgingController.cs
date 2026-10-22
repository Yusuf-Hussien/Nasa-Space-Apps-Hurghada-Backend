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

    /// <summary>List judging criteria (with their article/reference questions), optionally by challenge.</summary>
    [AllowAnonymous]
    [HttpGet("criteria")]
    public async Task<ActionResult<List<JudgingCriterionResponse>>> GetCriteria([FromQuery] int? challengeId = null)
    {
        var criteriaResult = await _judgingService.GetCriteriaAsync(challengeId);
        if (!criteriaResult.IsSuccess)
        {
            return criteriaResult.ToHttpResult(this, items => items.Select(JudgingCriterionResponse.From).ToList());
        }

        var publicationsResult = await _judgingService.GetPublicationsAsync();
        var publications = publicationsResult.IsSuccess ? publicationsResult.Value! : new List<Domain.Entities.CriterionPublication>();
        return criteriaResult.ToHttpResult(this, items => items
            .Select(criterion => JudgingCriterionResponse.From(criterion, publications, _currentUser.JudgeId))
            .ToList());
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("criteria")]
    public async Task<ActionResult<JudgingCriterionResponse>> CreateCriterion([FromBody] JudgingCriterionInput input)
    {
        var result = await _judgingService.CreateCriterionAsync(input);
        return result.ToHttpResult(this, JudgingCriterionResponse.From);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("criteria/{criterionId:int}")]
    public async Task<ActionResult<JudgingCriterionResponse>> UpdateCriterion(int criterionId, [FromBody] JudgingCriterionInput input)
    {
        var result = await _judgingService.UpdateCriterionAsync(criterionId, input);
        return result.ToHttpResult(this, JudgingCriterionResponse.From);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("criteria/{criterionId:int}")]
    public async Task<ActionResult<DeletedResponse>> DeleteCriterion(int criterionId)
    {
        var result = await _judgingService.DeleteCriterionAsync(criterionId);
        return result.ToHttpResult(this, _ => DeletedResponse.Of(criterionId));
    }

    /// <summary>
    /// Publish this judge's scores for the criterion so they appear on the live scoreboard.
    /// An Admin may pass ?judgeId=N to publish a specific judge, or omit it to
    /// force-publish every judge who has scores for the criterion.
    /// </summary>
    [Authorize(Policy = "JudgeOrAdminOnly")]
    [HttpPost("criteria/{criterionId:int}/publish")]
    public async Task<ActionResult<JudgingCriterionResponse>> PublishCriterion(int criterionId, [FromQuery] int? judgeId = null)
    {
        var result = await _judgingService.PublishCriterionAsync(criterionId, publish: true, _currentUser.Role, _currentUser.JudgeId, judgeId);
        var httpResult = result.ToHttpResult(this, criterion => JudgingCriterionResponse.From(criterion));
        if (result.IsSuccess)
        {
            await BroadcastStandingsAsync();
        }

        return httpResult;
    }

    /// <summary>Withdraw this judge's published scores for the criterion.</summary>
    [Authorize(Policy = "JudgeOrAdminOnly")]
    [HttpPost("criteria/{criterionId:int}/unpublish")]
    public async Task<ActionResult<JudgingCriterionResponse>> UnpublishCriterion(int criterionId, [FromQuery] int? judgeId = null)
    {
        var result = await _judgingService.PublishCriterionAsync(criterionId, publish: false, _currentUser.Role, _currentUser.JudgeId, judgeId);
        var httpResult = result.ToHttpResult(this, criterion => JudgingCriterionResponse.From(criterion));
        if (result.IsSuccess)
        {
            await BroadcastStandingsAsync();
        }

        return httpResult;
    }

    /// <summary>Public live scoreboard. Ranked by achieved percentage; criteria averaged across judges who published them.</summary>
    [AllowAnonymous]
    [HttpGet("standings")]
    public async Task<ActionResult<List<StandingsEntryResponse>>> GetStandings()
    {
        var result = await _judgingService.GetStandingsAsync();
        return result.ToHttpResult(this, rows => rows.Select(StandingsEntryResponse.From).ToList());
    }

    /// <summary>
    /// Submit per-question scores for a team. The judgeId comes from the token,
    /// never the body. Ad-hoc questions are allowed when criterionQuestionId is null.
    /// Re-submitting the same judge+team+question replaces the previous score.
    /// Broadcasts updated standings via SignalR.
    /// </summary>
    [Authorize(Policy = "JudgeOnly")]
    [HttpPost("scores")]
    public async Task<ActionResult<List<QuestionScoreResponse>>> SubmitScores([FromBody] SubmitScoresRequest request)
    {
        var judgeId = _currentUser.JudgeId;
        if (!judgeId.HasValue)
        {
            return StatusCode(403, new { error = new { code = "judging.no_judge_link", message = "Your account is not linked to a judge profile." } });
        }

        var result = await _judgingService.SubmitScoresAsync(request.TeamId, judgeId.Value, request.Scores);
        var submitted = result.IsSuccess
            ? result.Value!.Select(QuestionScoreResponse.From).ToList()
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
    public async Task<ActionResult<List<QuestionScoreResponse>>> GetScores([FromQuery] int teamId)
    {
        var result = await _judgingService.GetScoresForTeamAsync(teamId);
        return result.ToHttpResult(this, scores => scores.Select(QuestionScoreResponse.From).ToList());
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
