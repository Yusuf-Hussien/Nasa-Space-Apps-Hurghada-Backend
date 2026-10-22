using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly TeamService _teamService;

    public TeamsController(TeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeamSummaryResponse>>> GetTeams([FromQuery] string? search = null)
    {
        var result = await _teamService.SearchTeamsAsync(search);
        return result.ToHttpResult(
            this,
            teams => teams.Select(t => TeamSummaryResponse.From(t, User.IsStaff())).ToList());
    }

    /// <summary>
    /// Public team detail: challenge, project, and roster (student names only).
    /// Sensitive fields (username, contact details) are included only for staff callers.
    /// </summary>
    [HttpGet("{teamId:int}")]
    public async Task<ActionResult<TeamDetailResponse>> GetTeam(int teamId)
    {
        var isStaff = User.IsStaff();
        var result = await _teamService.GetTeamAsync(teamId);
        return result.ToHttpResult(this, team => TeamDetailResponse.From(team, isStaff));
    }

    /// <summary>Team roster. Staff-only: contains student names and contact details.</summary>
    [Authorize(Policy = "StaffOnly")]
    [HttpGet("{teamId:int}/students")]
    public async Task<ActionResult<List<StudentResponse>>> GetStudents(int teamId)
    {
        var result = await _teamService.GetStudentsAsync(teamId);
        return result.ToHttpResult(this, students => students.Select(StudentResponse.From).ToList());
    }

    /// <summary>Set or clear a team's logo. Body: <c>{ "image": "&lt;base64|url&gt;" }</c> (max 20 MB decoded).</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{teamId:int}/logo")]
    public async Task<ActionResult<TeamDetailResponse>> SetTeamLogo(int teamId, [FromBody] ImageInput input)
    {
        var isStaff = User.IsStaff();
        var result = await _teamService.SetLogoAsync(teamId, input.Image);
        return result.ToHttpResult(this, team => TeamDetailResponse.From(team, isStaff));
    }
}
