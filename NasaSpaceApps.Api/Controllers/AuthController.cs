using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
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
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginUnifiedAsync(
            request.Identifier, request.TeamId, request.Password, request.RememberMe);
        return result.ToHttpResult(this);
    }

    /// <summary>Staff login. Same contract as <see cref="Login"/> with identifier+password.</summary>
    [HttpPost("login/staff")]
    public async Task<ActionResult<LoginResponse>> LoginStaff([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginUnifiedAsync(
            request.Identifier, null, request.Password, request.RememberMe);
        return result.ToHttpResult(this);
    }

    /// <summary>Participant login. Same contract as <see cref="Login"/> with teamId+password.</summary>
    [HttpPost("login/participant")]
    public async Task<ActionResult<LoginResponse>> LoginParticipant([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginUnifiedAsync(
            null, request.TeamId, request.Password, request.RememberMe);
        return result.ToHttpResult(this);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<LoginResponse>> Me()
    {
        var result = await _authService.GetCurrentUserAsync(_currentUser);
        return result.ToHttpResult(this);
    }
}
