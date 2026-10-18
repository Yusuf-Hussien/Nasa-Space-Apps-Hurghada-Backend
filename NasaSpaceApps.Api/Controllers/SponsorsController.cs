using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult> GetSponsors([FromQuery] bool includeInactive = false)
    {
        var result = await _sponsorService.GetSponsorsAsync(includeInactive);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(sponsor => new
        {
            sponsorId = sponsor.SponsorId,
            name = sponsor.Name,
            logo = sponsor.Logo,
            link = sponsor.Link,
            tierId = sponsor.TierId,
            tier = sponsor.Tier == null ? null : new { tierId = sponsor.Tier.TierId, name = sponsor.Tier.Name, priority = sponsor.Tier.Priority, bannerEnabled = sponsor.Tier.BannerEnabled },
            active = sponsor.Active,
            displayOrder = sponsor.DisplayOrder
        }));
    }

    [HttpGet("{sponsorId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSponsor(int sponsorId)
    {
        var result = await _sponsorService.GetSponsorByIdAsync(sponsorId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var sponsor = result.Value!;
        return Ok(new
        {
            sponsorId = sponsor.SponsorId,
            name = sponsor.Name,
            logo = sponsor.Logo,
            link = sponsor.Link,
            tierId = sponsor.TierId,
            tier = sponsor.Tier == null ? null : new { tierId = sponsor.Tier.TierId, name = sponsor.Tier.Name, priority = sponsor.Tier.Priority, bannerEnabled = sponsor.Tier.BannerEnabled },
            active = sponsor.Active,
            displayOrder = sponsor.DisplayOrder
        });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> AddSponsor([FromBody] SponsorInputDto input)
    {
        var result = await _sponsorService.AddSponsorAsync(new SponsorInput(
            input.Name, input.Logo, input.Link, input.TierId, input.Active, input.DisplayOrder));
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var sponsor = result.Value!;
        return Ok(new
        {
            sponsorId = sponsor.SponsorId,
            name = sponsor.Name,
            logo = sponsor.Logo,
            link = sponsor.Link,
            tierId = sponsor.TierId,
            active = sponsor.Active,
            displayOrder = sponsor.DisplayOrder
        });
    }

    [HttpPut("{sponsorId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> UpdateSponsor(int sponsorId, [FromBody] SponsorUpdateDto input)
    {
        var result = await _sponsorService.UpdateSponsorAsync(sponsorId, new SponsorUpdateInput(
            input.Name, input.Logo, input.Link, input.TierId, input.Active, input.DisplayOrder));
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var sponsor = result.Value!;
        return Ok(new
        {
            sponsorId = sponsor.SponsorId,
            name = sponsor.Name,
            logo = sponsor.Logo,
            link = sponsor.Link,
            tierId = sponsor.TierId,
            active = sponsor.Active,
            displayOrder = sponsor.DisplayOrder
        });
    }

    [HttpDelete("{sponsorId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteSponsor(int sponsorId)
    {
        var result = await _sponsorService.DeleteSponsorAsync(sponsorId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { sponsorId, deleted = true });
    }

    [HttpGet("tiers")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSponsorTiers()
    {
        var result = await _sponsorService.GetSponsorTiersAsync();
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(t => new
        {
            tierId = t.TierId,
            name = t.Name,
            priority = t.Priority,
            bannerEnabled = t.BannerEnabled
        }));
    }

    [HttpGet("tiers/{tierId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSponsorTier(int tierId)
    {
        var result = await _sponsorService.GetSponsorTierByIdAsync(tierId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var t = result.Value!;
        return Ok(new
        {
            tierId = t.TierId,
            name = t.Name,
            priority = t.Priority,
            bannerEnabled = t.BannerEnabled
        });
    }

    [HttpPost("tiers")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> AddSponsorTier([FromBody] SponsorTierInputDto input)
    {
        var result = await _sponsorService.AddSponsorTierAsync(new SponsorTierInput(input.Name, input.Priority, input.BannerEnabled));
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var t = result.Value!;
        return Ok(new
        {
            tierId = t.TierId,
            name = t.Name,
            priority = t.Priority,
            bannerEnabled = t.BannerEnabled
        });
    }

    [HttpPut("tiers/{tierId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> UpdateSponsorTier(int tierId, [FromBody] SponsorTierUpdateDto input)
    {
        var result = await _sponsorService.UpdateSponsorTierAsync(tierId, new SponsorTierUpdateInput(input.Name, input.Priority, input.BannerEnabled));
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        var t = result.Value!;
        return Ok(new
        {
            tierId = t.TierId,
            name = t.Name,
            priority = t.Priority,
            bannerEnabled = t.BannerEnabled
        });
    }

    [HttpDelete("tiers/{tierId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteSponsorTier(int tierId)
    {
        var result = await _sponsorService.DeleteSponsorTierAsync(tierId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new { tierId, deleted = true });
    }
}