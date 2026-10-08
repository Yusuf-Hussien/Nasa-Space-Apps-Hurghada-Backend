using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Data;
using NASAHackathon.API.Models;

namespace NASAHackathon.API.Controllers
{
    [ApiController]
    [Route("api/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
        {
            var teamIdClaim = User.FindFirst("teamId")?.Value;
            if (!int.TryParse(teamIdClaim, out var teamId)) return Unauthorized();

            var req = new ServiceRequest
            {
                TeamId = teamId,
                Type = dto.RequestType,
                Message = dto.Message,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.ServiceRequests.Add(req);
            await _context.SaveChangesAsync();
            return CreatedAtAction(null, new { requestId = req.RequestId, teamId = req.TeamId, requestType = req.Type, message = req.Message, location = dto.Location, status = req.Status, createdAt = req.CreatedAt });
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> My([FromQuery] string? status, [FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var teamIdClaim = User.FindFirst("teamId")?.Value;
            if (!int.TryParse(teamIdClaim, out var teamId)) return Unauthorized();

            var q = _context.ServiceRequests.Where(r => r.TeamId == teamId);
            if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
            var items = await q.OrderByDescending(r => r.CreatedAt).Skip(offset).Take(limit).ToListAsync();
            var total = await q.CountAsync();
            return Ok(new { items = items.Select(r => new { requestId = r.RequestId, requestType = r.Type, status = r.Status, message = r.Message, location = (string?)null, createdAt = r.CreatedAt, resolvedAt = r.ResolvedAt }), total });
        }
    }

    public class CreateRequestDto
    {
        public string RequestType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
