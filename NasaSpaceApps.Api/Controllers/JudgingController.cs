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

    /// <summary>List the global judging criteria and their optional reference questions.</summary>
    [AllowAnonymous]
    [HttpGet("criteria")]
    public async Task<ActionResult<List<JudgingCriterionResponse>>> GetCriteria([FromQuery] int? teamId = null)
    {
        var criteriaResult = await _judgingService.GetCriteriaAsync();
        if (!criteriaResult.IsSuccess)
        {
            return criteriaResult.ToHttpResult(this, items => items.Select(JudgingCriterionResponse.From).ToList());
        }

        var publicationsResult = await _judgingService.GetPublicationsAsync(teamId: teamId);
        var publications = publicationsResult.IsSuccess ? publicationsResult.Value! : new List<Domain.Entities.CriterionPublication>();
        return criteriaResult.ToHttpResult(this, items => items
            .Select(criterion => JudgingCriterionResponse.From(criterion, publications, _currentUser.JudgeId, teamId))
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
    /// An Admin may pass ?judgeId=N and/or ?teamId=N to target specific scores, or omit
    /// both to force-publish every scored judge/team pair for the criterion.
    /// </summary>
    [Authorize(Policy = "JudgeOrAdminOnly")]
    [HttpPost("criteria/{criterionId:int}/publish")]
    public async Task<ActionResult<JudgingCriterionResponse>> PublishCriterion(
        int criterionId,
        [FromQuery] int? judgeId = null,
        [FromQuery] int? teamId = null)
    {
        var result = await _judgingService.PublishCriterionAsync(
            criterionId,
            publish: true,
            _currentUser.Role,
            _currentUser.JudgeId,
            judgeId,
            teamId);
        var publications = await GetPublicationScopeAsync(result.IsSuccess, criterionId, teamId);
        var httpResult = result.ToHttpResult(this, criterion =>
            JudgingCriterionResponse.From(criterion, publications, _currentUser.JudgeId, teamId));
        if (result.IsSuccess)
        {
            await BroadcastStandingsAsync();
        }

        return httpResult;
    }

    /// <summary>Withdraw this judge's published scores for the criterion, optionally scoped to a team.</summary>
    [Authorize(Policy = "JudgeOrAdminOnly")]
    [HttpPost("criteria/{criterionId:int}/unpublish")]
    public async Task<ActionResult<JudgingCriterionResponse>> UnpublishCriterion(
        int criterionId,
        [FromQuery] int? judgeId = null,
        [FromQuery] int? teamId = null)
    {
        var result = await _judgingService.PublishCriterionAsync(
            criterionId,
            publish: false,
            _currentUser.Role,
            _currentUser.JudgeId,
            judgeId,
            teamId);
        var publications = await GetPublicationScopeAsync(result.IsSuccess, criterionId, teamId);
        var httpResult = result.ToHttpResult(this, criterion =>
            JudgingCriterionResponse.From(criterion, publications, _currentUser.JudgeId, teamId));
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
    /// Submit and immediately publish one criterion score for a team. The judgeId
    /// comes from the token. Re-submitting the same judge+team+criterion replaces
    /// the score and republishes it. Broadcasts score and standings updates.
    /// </summary>
    [Authorize(Policy = "JudgeOnly")]
    [HttpPost("scores/{criterionId:int}")]
    public async Task<ActionResult<SubmitCriterionScoreResponse>> SubmitCriterionScore(
        int criterionId,
        [FromBody] SubmitCriterionScoreRequest request)
    {
        var judgeId = _currentUser.JudgeId;
        if (!judgeId.HasValue)
        {
            return StatusCode(403, new { error = new { code = "judging.no_judge_link", message = "Your account is not linked to a judge profile." } });
        }

        var submitResult = await _judgingService.SubmitScoresAsync(
            request.TeamId,
            judgeId.Value,
            new List<CriterionScoreInput>
            {
                new(criterionId, request.Score, request.Comment)
            });
        if (!submitResult.IsSuccess)
        {
            return submitResult.ToHttpResult(this, _ => default(SubmitCriterionScoreResponse)!);
        }

        var publishResult = await _judgingService.PublishCriterionAsync(
            criterionId,
            publish: true,
            _currentUser.Role,
            judgeId,
            judgeId,
            request.TeamId);
        if (!publishResult.IsSuccess)
        {
            return publishResult.ToHttpResult(this, _ => default(SubmitCriterionScoreResponse)!);
        }

        var publicationsResult = await _judgingService.GetPublicationsAsync(criterionId, teamId: request.TeamId);
        var publications = publicationsResult.IsSuccess
            ? publicationsResult.Value!
            : new List<Domain.Entities.CriterionPublication>();
        var score = JudgeScoreResponse.From(submitResult.Value!.Single());
        var criterion = JudgingCriterionResponse.From(
            publishResult.Value!,
            publications,
            judgeId,
            request.TeamId);

        await _hubContext.Clients.All.SendAsync(
            JudgingHub.EventScoresSubmitted,
            new { teamId = request.TeamId, judgeId, scores = new[] { score } });

        await BroadcastStandingsAsync();
        return Ok(new SubmitCriterionScoreResponse(score, criterion));
    }

    /// <summary>
    /// Submit one 1.0-5.0 score per criterion for a team. The judgeId comes from
    /// the token, never the body. Re-submitting the same judge+team+criterion
    /// replaces the previous score. Broadcasts updated standings via SignalR.
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

        var result = await _judgingService.SubmitScoresAsync(request.TeamId, judgeId.Value, request.Scores);
        var submitted = result.IsSuccess
            ? result.Value!.Select(score => JudgeScoreResponse.From(score)).ToList()
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
        return result.ToHttpResult(this, scores => scores.Select(score => JudgeScoreResponse.From(score)).ToList());
    }

    private async Task<List<Domain.Entities.CriterionPublication>> GetPublicationScopeAsync(
        bool succeeded,
        int criterionId,
        int? teamId)
    {
        if (!succeeded)
        {
            return new List<Domain.Entities.CriterionPublication>();
        }

        var result = await _judgingService.GetPublicationsAsync(criterionId, teamId: teamId);
        return result.IsSuccess ? result.Value! : new List<Domain.Entities.CriterionPublication>();
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
