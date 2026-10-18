using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SponsorsController : ControllerBase
{
    private readonly SponsorService _sponsorService;

    public SponsorsController(SponsorService sponsorService)
    {
        _sponsorService = sponsorService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SponsorResponse>>> GetSponsors([FromQuery] bool includeInactive = false)
    {
        var result = await _sponsorService.GetSponsorsAsync(includeInactive);
        return result.ToHttpResult(this, sponsors => sponsors.Select(s => SponsorResponse.From(s)).ToList());
    }

    [HttpGet("{sponsorId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<SponsorResponse>> GetSponsor(int sponsorId)
    {
        var result = await _sponsorService.GetSponsorByIdAsync(sponsorId);
        return result.ToHttpResult(this, s => SponsorResponse.From(s));
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SponsorResponse>> AddSponsor([FromBody] SponsorInput input)
    {
        var result = await _sponsorService.AddSponsorAsync(input);
        return result.ToHttpResult(this, s => SponsorResponse.From(s, includeTier: false));
    }

    [HttpPut("{sponsorId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SponsorResponse>> UpdateSponsor(int sponsorId, [FromBody] SponsorInput input)
    {
        var result = await _sponsorService.UpdateSponsorAsync(sponsorId, input);
        return result.ToHttpResult(this, s => SponsorResponse.From(s, includeTier: false));
    }

    [HttpDelete("{sponsorId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<DeletedResponse>> DeleteSponsor(int sponsorId)
    {
        var result = await _sponsorService.DeleteSponsorAsync(sponsorId);
        return result.ToHttpResult(this, _ => DeletedResponse.Of(sponsorId));
    }

    [HttpGet("tiers")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SponsorTierResponse>>> GetSponsorTiers()
    {
        var result = await _sponsorService.GetSponsorTiersAsync();
        return result.ToHttpResult(this, tiers => tiers.Select(SponsorTierResponse.From).ToList());
    }

    [HttpGet("tiers/{tierId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<SponsorTierResponse>> GetSponsorTier(int tierId)
    {
        var result = await _sponsorService.GetSponsorTierByIdAsync(tierId);
        return result.ToHttpResult(this, SponsorTierResponse.From);
    }

    [HttpPost("tiers")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SponsorTierResponse>> AddSponsorTier([FromBody] SponsorTierInput input)
    {
        var result = await _sponsorService.AddSponsorTierAsync(input);
        return result.ToHttpResult(this, SponsorTierResponse.From);
    }

    [HttpPut("tiers/{tierId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SponsorTierResponse>> UpdateSponsorTier(int tierId, [FromBody] SponsorTierInput input)
    {
        var result = await _sponsorService.UpdateSponsorTierAsync(tierId, input);
        return result.ToHttpResult(this, SponsorTierResponse.From);
    }

    [HttpDelete("tiers/{tierId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<DeletedResponse>> DeleteSponsorTier(int tierId)
    {
        var result = await _sponsorService.DeleteSponsorTierAsync(tierId);
        return result.ToHttpResult(this, _ => DeletedResponse.Of(tierId));
    }
}
