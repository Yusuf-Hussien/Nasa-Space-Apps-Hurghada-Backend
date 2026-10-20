using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

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

    /// <summary>Staff login: identifier (email or username) + password.</summary>
    [HttpPost("login/staff")]
    public async Task<ActionResult<StaffLoginResponse>> LoginStaff([FromBody] StaffLoginRequest request)
    {
        var result = await _authService.LoginStaffAsync(
            request.Identifier, request.Password, request.RememberMe);
        return result.ToHttpResult(this, StaffLoginResponse.From);
    }

    /// <summary>Participant login: team username + password. Returns the team's details, challenge, and roster.</summary>
    [HttpPost("login/participant")]
    public async Task<ActionResult<ParticipantLoginResponse>> LoginParticipant([FromBody] ParticipantLoginRequest request)
    {
        var result = await _authService.LoginParticipantAsync(
            request.Username, request.Password, request.RememberMe);
        return result.ToHttpResult<LoginResponse, ParticipantLoginResponse>(this, value =>
            ParticipantLoginResponse.From(value.AccessToken, value.Team!, value.TeamStudents ?? []));
    }

    /// <summary>Current authenticated account. Returns the same shape as the matching login route.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var result = await _authService.GetCurrentUserAsync(_currentUser);
        return result.ToHttpResult<LoginResponse, object>(this, ToRoleSpecificResponse);
    }

    /// <summary>Picks the response shape from the account's role, so neither role
    /// receives the other's fields.</summary>
    private static object ToRoleSpecificResponse(LoginResponse value) =>
        string.Equals(value.Role, AppRoles.Participant, StringComparison.OrdinalIgnoreCase)
            ? ParticipantLoginResponse.From(value.AccessToken, value.Team!, value.TeamStudents ?? [])
            : StaffLoginResponse.From(value);
}
