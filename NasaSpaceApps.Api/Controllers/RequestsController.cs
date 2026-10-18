using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Api.Hubs;
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
    private readonly IHubContext<RequestsHub> _hubContext;

    public RequestsController(
        RequestService requestService,
        ICurrentUser currentUser,
        IHubContext<RequestsHub> hubContext)
    {
        _requestService = requestService;
        _currentUser = currentUser;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceRequestResponse>>> GetRequests(
        [FromQuery] int? teamId = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        // Participants may only view their own team's requests
        if (_currentUser.Role == AppRoles.Participant && _currentUser.TeamId.HasValue)
        {
            teamId = _currentUser.TeamId.Value;
        }

        var result = await _requestService.GetRequestsAsync(teamId, type, status);
        return result.ToHttpResult(this, requests => requests.Select(ServiceRequestResponse.From).ToList());
    }

    /// <summary>
    /// Create a request (organizer/mentor). For participants the teamId is taken
    /// from the token; staff may specify a teamId on behalf of a team.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ServiceRequestResponse>> CreateRequest([FromBody] CreateRequestRequest request)
    {
        var teamId = request.TeamId;
        if (_currentUser.Role == AppRoles.Participant && _currentUser.TeamId.HasValue)
        {
            teamId = _currentUser.TeamId.Value; // always use token team for participants
        }
        else if (!_currentUser.IsStaff)
        {
            return StatusCode(403, new { error = new { code = "request.forbidden", message = "Not allowed to create requests." } });
        }

        var result = await _requestService.CreateRequestAsync(teamId, request.Type, request.Message);

        var response = result.IsSuccess ? ServiceRequestResponse.From(result.Value!) : null;
        if (response is not null)
        {
            await BroadcastAsync(RequestsHub.EventRequestCreated, response);
        }

        return result.ToHttpResult(this, _ => response!);
    }

    [Authorize(Policy = "StaffOnly")]
    [HttpPatch("{requestId:int}")]
    public async Task<ActionResult<ServiceRequestResponse>> ResolveRequest(
        int requestId,
        [FromBody] ResolveRequestRequest request)
    {
        var result = await _requestService.ResolveRequestAsync(requestId, request.Status);

        var response = result.IsSuccess ? ServiceRequestResponse.From(result.Value!) : null;
        if (response is not null)
        {
            await BroadcastAsync(RequestsHub.EventRequestUpdated, response);
        }

        return result.ToHttpResult(this, _ => response!);
    }

    [HttpDelete("{requestId:int}")]
    public async Task<ActionResult<DeletedResponse>> DeleteRequest(int requestId)
    {
        var teamId = _currentUser.IsStaff ? (int?)null : _currentUser.TeamId;
        var result = await _requestService.DeleteRequestAsync(requestId, teamId, _currentUser.IsStaff);

        if (result.IsSuccess)
        {
            await BroadcastAsync(RequestsHub.EventRequestDeleted, requestId);
        }

        return result.ToHttpResult(this, _ => DeletedResponse.Of(requestId));
    }

    private Task BroadcastAsync(string eventName, object payload) =>
        _hubContext.Clients.Group(RequestsHub.OrganizerGroup).SendAsync(eventName, payload);
}
