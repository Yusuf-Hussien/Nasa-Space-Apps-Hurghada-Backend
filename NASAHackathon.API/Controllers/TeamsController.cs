using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Data;
using System.Security.Claims;

namespace NASAHackathon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var teamIdClaim = User.FindFirst("teamId")?.Value;
            if (!int.TryParse(teamIdClaim, out var teamId))
                return Unauthorized();

            var team = await _context.Teams
                .Include(t => t.Challenge)
                .Include(t => t.Students)
                .FirstOrDefaultAsync(t => t.TeamId == teamId);

            if (team == null) return NotFound();

            var result = new
            {
                teamId = team.TeamId,
                name = team.Name,
                challengeId = team.ChallengeId,
                challenge = team.Challenge == null ? null : new { challengeId = team.Challenge.ChallengeId, title = team.Challenge.Title, summary = team.Challenge.Description, season = team.Challenge.Season, isPublished = true },
                students = team.Students.Select(s => new { studentId = s.StudentId, fullName = s.Name, email = s.Email, contactNumber = s.Contact, status = s.Status })
            };

            return Ok(result);
        }
    }
}
