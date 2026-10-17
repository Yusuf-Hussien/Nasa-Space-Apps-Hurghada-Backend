using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult> GetChallenges()
    {
        var result = await _challengeService.GetChallengesAsync();
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(challenge => new
        {
            challengeId = challenge.ChallengeId,
            title = challenge.Title,
            description = challenge.Description,
            source = challenge.Source,
            season = challenge.Season
        }));
    }

    [HttpGet("{challengeId:int}")]
    public async Task<ActionResult> GetChallenge(int challengeId)
    {
        var result = await _challengeService.GetChallengeAsync(challengeId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new
        {
            challengeId = result.Value!.ChallengeId,
            title = result.Value.Title,
            description = result.Value.Description,
            source = result.Value.Source,
            season = result.Value.Season
        });
    }
}
