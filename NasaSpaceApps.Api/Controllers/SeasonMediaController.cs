using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SeasonMediaController : ControllerBase
{
    private readonly SeasonMediaService _seasonMediaService;

    public SeasonMediaController(SeasonMediaService seasonMediaService)
    {
        _seasonMediaService = seasonMediaService;
    }

    /// <summary>Public landing-page gallery. Past-season photos/footage, filtered by season on the client.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<SeasonMediaResponse>>> GetMedia([FromQuery] bool includeInactive = false)
    {
        var result = await _seasonMediaService.GetMediaAsync(includeInactive);
        return result.ToHttpResult(this, items => items.Select(SeasonMediaResponse.From).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{mediaId:int}")]
    public async Task<ActionResult<SeasonMediaResponse>> GetMedia(int mediaId)
    {
        var result = await _seasonMediaService.GetMediaByIdAsync(mediaId);
        return result.ToHttpResult(this, SeasonMediaResponse.From);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<SeasonMediaResponse>> AddMedia([FromBody] SeasonMediaInput input)
    {
        var result = await _seasonMediaService.AddAsync(input);
        return result.ToHttpResult(this, SeasonMediaResponse.From);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{mediaId:int}")]
    public async Task<ActionResult<SeasonMediaResponse>> UpdateMedia(int mediaId, [FromBody] SeasonMediaInput input)
    {
        var result = await _seasonMediaService.UpdateAsync(mediaId, input);
        return result.ToHttpResult(this, SeasonMediaResponse.From);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{mediaId:int}")]
    public async Task<ActionResult<DeletedResponse>> DeleteMedia(int mediaId)
    {
        var result = await _seasonMediaService.DeleteAsync(mediaId);
        return result.ToHttpResult(this, _ => DeletedResponse.Of(mediaId));
    }
}
