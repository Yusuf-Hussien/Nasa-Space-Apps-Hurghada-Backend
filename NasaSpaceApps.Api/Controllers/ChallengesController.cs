using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly ChallengeService _challengeService;

    public ChallengesController(ChallengeService challengeService)
    {
        _challengeService = challengeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChallengeResponse>>> GetChallenges()
    {
        var result = await _challengeService.GetChallengesAsync();
        return result.ToHttpResult(this, challenges => challenges.Select(ChallengeResponse.From).ToList());
    }

    [HttpGet("{challengeId:int}")]
    public async Task<ActionResult<ChallengeResponse>> GetChallenge(int challengeId)
    {
        var result = await _challengeService.GetChallengeAsync(challengeId);
        return result.ToHttpResult(this, ChallengeResponse.From);
    }

    /// <summary>Set or clear a challenge's image. Body: <c>{ "image": "&lt;base64|url&gt;" }</c> (max 20 MB decoded).</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{challengeId:int}/image")]
    public async Task<ActionResult<ChallengeResponse>> SetChallengeImage(int challengeId, [FromBody] ImageInput input)
    {
        var result = await _challengeService.SetImageAsync(challengeId, input.Image);
        return result.ToHttpResult(this, ChallengeResponse.From);
    }
}
