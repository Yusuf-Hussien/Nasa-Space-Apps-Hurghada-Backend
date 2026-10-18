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
}
