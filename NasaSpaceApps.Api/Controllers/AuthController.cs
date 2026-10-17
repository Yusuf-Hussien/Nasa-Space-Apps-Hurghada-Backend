using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(AuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Unified login. Staff: { identifier } (email or username) + password.
    /// Participant: { teamId } + password.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginUnifiedAsync(request.Identifier, request.TeamId, request.Password);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var value = result.Value!;
        return Ok(new
        {
            accessToken = value.AccessToken,
            role = value.Role,
            name = value.Name,
            teamId = value.TeamId,
            username = value.Username,
            email = value.Email,
            staffId = value.StaffId,
            judgeId = value.JudgeId
        });
    }

    /// <summary>Back-compat alias: username+password → always treated as staff identifier.</summary>
    [HttpPost("login/staff")]
    public async Task<ActionResult> LoginStaff([FromBody] StaffLoginRequest request)
    {
        var result = await _authService.LoginUnifiedAsync(request.Identifier, null, request.Password);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var value = result.Value!;
        return Ok(new
        {
            accessToken = value.AccessToken,
            role = value.Role,
            name = value.Name,
            username = value.Username,
            email = value.Email,
            staffId = value.StaffId,
            judgeId = value.JudgeId
        });
    }

    /// <summary>Back-compat alias: teamId+password → participant.</summary>
    [HttpPost("login/participant")]
    public async Task<ActionResult> LoginParticipant([FromBody] ParticipantLoginRequest request)
    {
        var result = await _authService.LoginUnifiedAsync(null, request.TeamId, request.Password);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var value = result.Value!;
        return Ok(new
        {
            accessToken = value.AccessToken,
            role = value.Role,
            name = value.Name,
            teamId = value.TeamId,
            username = value.Username
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult> Me()
    {
        var result = await _authService.GetCurrentUserAsync(_currentUser);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var value = result.Value!;
        return Ok(new
        {
            role = value.Role,
            name = value.Name,
            teamId = value.TeamId,
            username = value.Username,
            email = value.Email,
            staffId = value.StaffId,
            judgeId = value.JudgeId
        });
    }
}
