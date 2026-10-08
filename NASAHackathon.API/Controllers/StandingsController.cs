using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Data;

namespace NASAHackathon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StandingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StandingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int limit = 50)
        {
            var teams = await _context.Teams
                .Include(t => t.JudgeScores)
                .ThenInclude(js => js.Criterion)
                .Include(t => t.Project)
                .ToListAsync();

            var items = teams.Select(t => new
            {
                teamId = t.TeamId,
                teamName = t.Name,
                totalScore = t.JudgeScores.Sum(js => js.Score * (js.Criterion.Weight ?? 1m)),
                rank = 0,
                projectTitle = t.Project?.Title
            }).OrderByDescending(x => x.totalScore).Take(limit).ToList();

            for (int i = 0; i < items.Count; i++) items[i] = items[i] with { rank = i + 1 };

            return Ok(new { items, updatedAt = DateTime.UtcNow });
        }
    }
}
