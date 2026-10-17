using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly RequestService _requestService;

    public RequestsController(RequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet]
    public async Task<ActionResult> GetRequests([FromQuery] int? teamId = null, [FromQuery] string? type = null, [FromQuery] string? status = null)
    {
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
            createdAt = request.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<ActionResult> CreateRequest([FromBody] CreateRequestRequest request)
    {
        var result = await _requestService.CreateRequestAsync(request.TeamId, request.Type, request.Message);
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
}

public sealed record CreateRequestRequest(int TeamId, string Type, string Message);
