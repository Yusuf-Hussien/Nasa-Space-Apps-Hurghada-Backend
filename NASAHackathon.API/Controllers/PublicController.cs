using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Data;
using System.Text.Json;

namespace NASAHackathon.API.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        [HttpGet("event")]
        public IActionResult Event()
        {
            return Ok(new { @event = new { name = "NASA Space Apps", season = "2025-2026", description = "NASA Space Apps Challenge Hurghada" }, coreTeam = new List<object>(), media = new List<object>() });
        }
    }

    [ApiController]
    [Route("api/challenges")]
    public class ChallengesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChallengesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? season, [FromQuery] bool? published)
        {
            var q = _context.Challenges.AsQueryable();
            if (!string.IsNullOrEmpty(season)) q = q.Where(c => c.Season == season);
            var items = await q.ToListAsync();
            return Ok(new { items = items.Select(c => new { challengeId = c.ChallengeId, title = c.Title, summary = c.Description, source = c.Source, season = c.Season, isPublished = published ?? true, metadataJson = Parse(c.Metadata) }) });
        }

        private object? Parse(string? m)
        {
            if (string.IsNullOrEmpty(m)) return null;
            try { return JsonSerializer.Deserialize<object>(m); } catch { return m; }
        }
    }
}
