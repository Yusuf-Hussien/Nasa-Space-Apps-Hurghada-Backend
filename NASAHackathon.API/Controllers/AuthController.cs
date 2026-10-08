using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Data;
using NASAHackathon.API.DTOs.Auth;
using NASAHackathon.API.Services;

namespace NASAHackathon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(ApplicationDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TeamLoginRequest request)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Username == request.Username);
            if (team == null)
                return Unauthorized(new { error = new { code = "invalid_credentials", message = "Invalid username or password" }, timestamp = DateTime.UtcNow });

            // For now simple hash comparison placeholder; use proper hashing in prod
            // In real app verify BCrypt/Argon2
            if (team.PasswordHash != request.Password)
                return Unauthorized(new { error = new { code = "invalid_credentials", message = "Invalid username or password" }, timestamp = DateTime.UtcNow });

            var token = _jwt.GenerateTeamToken(team);
            return Ok(new TeamLoginResponse
            {
                Token = token,
                Team = new TeamAuthDto
                {
                    TeamId = team.TeamId,
                    Name = team.Name,
                    Username = team.Username,
                    ChallengeId = team.ChallengeId,
                    Status = team.Status
                }
            });
        }
    }
}
