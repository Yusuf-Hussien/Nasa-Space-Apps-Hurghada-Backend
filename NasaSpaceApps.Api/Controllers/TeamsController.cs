using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult> GetTeams([FromQuery] string? search = null)
    {
        var result = await _teamService.SearchTeamsAsync(search);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(team => new
        {
            teamId = team.TeamId,
            name = team.Name,
            username = team.Username,
            status = team.Status
        }));
    }

    [HttpGet("{teamId:int}")]
    public async Task<ActionResult> GetTeam(int teamId)
    {
        var result = await _teamService.GetTeamAsync(teamId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new
        {
            teamId = result.Value!.TeamId,
            name = result.Value.Name,
            username = result.Value.Username,
            status = result.Value.Status,
            challengeId = result.Value.ChallengeId
        });
    }

    [HttpGet("{teamId:int}/students")]
    public async Task<ActionResult> GetStudents(int teamId)
    {
        var result = await _teamService.GetStudentsAsync(teamId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(student => new
        {
            studentId = student.StudentId,
            name = student.Name,
            email = student.Email,
            contact = student.Contact,
            status = student.Status
        }));
    }
}
