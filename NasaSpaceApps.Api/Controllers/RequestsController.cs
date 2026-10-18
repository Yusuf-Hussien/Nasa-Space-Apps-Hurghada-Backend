using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly RequestService _requestService;
    private readonly ICurrentUser _currentUser;

    public RequestsController(RequestService requestService, ICurrentUser currentUser)
    {
        _requestService = requestService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult> GetRequests([FromQuery] int? teamId = null, [FromQuery] string? type = null, [FromQuery] string? status = null)
    {
        // Participants may only view their own team's requests
        if (_currentUser.Role == AppRoles.Participant && _currentUser.TeamId.HasValue)
        {
            teamId = _currentUser.TeamId.Value;
        }

        var result = await _requestService.GetRequestsAsync(teamId, type, status);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(request => new
        {
            requestId = request.RequestId,
            teamId = request.TeamId,
            type = request.Type,
            status = request.Status,
            message = request.Message,
            createdAt = request.CreatedAt,
            resolvedAt = request.ResolvedAt
        }));
    }

    /// <summary>
    /// Create a request (organizer/mentor). For participants the teamId is taken
    /// from the token; staff may specify a teamId on behalf of a team.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateRequest([FromBody] CreateRequestRequest request)
    {
        var teamId = request.TeamId;
        if (_currentUser.Role == AppRoles.Participant && _currentUser.TeamId.HasValue)
        {
            teamId = _currentUser.TeamId.Value; // always use token team for participants
        }
        else if (!_currentUser.IsStaff)
        {
            return StatusCode(403, new { code = "request.forbidden", message = "Not allowed to create requests." });
        }

        var result = await _requestService.CreateRequestAsync(teamId, request.Type, request.Message);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new
        {
            requestId = result.Value!.RequestId,
            teamId = result.Value.TeamId,
            type = result.Value.Type,
            status = result.Value.Status,
            createdAt = result.Value.CreatedAt,
            message = result.Value.Message
        });
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpPatch("{requestId:int}")]
    public async Task<ActionResult> ResolveRequest(int requestId, [FromBody] ResolveRequestRequest request)
    {
        var result = await _requestService.ResolveRequestAsync(requestId, request.Status);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new
        {
            requestId = result.Value!.RequestId,
            teamId = result.Value.TeamId,
            type = result.Value.Type,
            status = result.Value.Status,
            message = result.Value.Message,
            createdAt = result.Value.CreatedAt,
            resolvedAt = result.Value.ResolvedAt
        });
    }

    [HttpDelete("{requestId:int}")]
    public async Task<ActionResult> DeleteRequest(int requestId)
    {
        var teamId = _currentUser.IsStaff ? (int?)null : _currentUser.TeamId;
        var result = await _requestService.DeleteRequestAsync(requestId, teamId, _currentUser.IsStaff);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { requestId, deleted = true });
    }
}
