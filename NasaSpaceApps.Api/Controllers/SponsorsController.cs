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
    public async Task<ActionResult> GetSponsors()
    {
        var result = await _sponsorService.GetSponsorsAsync();
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
            active = sponsor.Active,
            displayOrder = sponsor.DisplayOrder
        }));
    }
}
