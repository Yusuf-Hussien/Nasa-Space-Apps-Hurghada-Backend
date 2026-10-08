using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Data;

namespace NASAHackathon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SponsorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("banner")]
        public async Task<IActionResult> Banner([FromQuery] int? tier, [FromQuery] bool? active)
        {
            var q = _context.Sponsors.AsQueryable();
            if (tier.HasValue) q = q.Where(s => s.TierId == tier.Value);
            if (active.HasValue) q = q.Where(s => s.Active == active.Value);
            var items = await q.OrderBy(s => s.DisplayOrder).ToListAsync();
            return Ok(new { items = items.Select(s => new { sponsorId = s.SponsorId, name = s.Name, logoUrl = s.Logo, websiteUrl = s.Link, tierId = s.TierId, displayOrder = s.DisplayOrder, isActive = s.Active }) });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? active, [FromQuery] int? tier)
        {
            var q = _context.Sponsors.Include(s => s.Tier).AsQueryable();
            if (active.HasValue) q = q.Where(s => s.Active == active.Value);
            if (tier.HasValue) q = q.Where(s => s.TierId == tier.Value);
            var items = await q.OrderBy(s => s.Tier.Priority).ThenBy(s => s.DisplayOrder).ToListAsync();
            return Ok(new { items = items.Select(s => new { sponsorId = s.SponsorId, name = s.Name, logoUrl = s.Logo, websiteUrl = s.Link, tierId = s.TierId, displayOrder = s.DisplayOrder, isActive = s.Active, tier = s.Tier == null ? null : new { tierId = s.Tier.TierId, name = s.Tier.Name, priority = s.Tier.Priority, bannerEnabled = s.Tier.BannerEnabled } }) });
        }
    }
}
