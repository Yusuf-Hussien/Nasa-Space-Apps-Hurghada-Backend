using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Data;
using NASAHackathon.API.Models;

namespace NASAHackathon.API.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GameController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("questions")]
        public async Task<IActionResult> GetQuestions([FromQuery] int? challengeId, [FromQuery] string? type)
        {
            var q = _context.Questions.Where(x => x.IsActive);
            if (challengeId.HasValue) q = q.Where(x => x.ChallengeId == challengeId);
            if (!string.IsNullOrEmpty(type)) q = q.Where(x => x.Type == type);
            var items = await q.ToListAsync();
            return Ok(new { items = items.Select(x => new { questionId = x.QuestionId, challengeId = x.ChallengeId, questionType = x.Type, questionText = x.Text, optionsJson = ParseOptions(x.Options), isActive = x.IsActive }) });
        }

        [Authorize]
        [HttpPost("answers")]
        public async Task<IActionResult> PostAnswer([FromBody] AnswerDto dto)
        {
            var teamIdClaim = User.FindFirst("teamId")?.Value;
            if (!int.TryParse(teamIdClaim, out var teamId)) return Unauthorized();

            var question = await _context.Questions.FindAsync(dto.QuestionId);
            if (question == null) return NotFound();

            bool? isCorrect = null;
            if (!string.IsNullOrEmpty(question.CorrectAnswer))
            {
                isCorrect = string.Equals(question.CorrectAnswer.Trim(), dto.AnswerText?.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            var part = new GameParticipation
            {
                QuestionId = dto.QuestionId,
                TeamId = teamId,
                Answer = dto.AnswerText,
                Correct = isCorrect,
                SubmittedAt = DateTime.UtcNow
            };
            _context.GameParticipations.Add(part);
            await _context.SaveChangesAsync();
            return CreatedAtAction(null, new { participationId = part.ParticipationId, questionId = part.QuestionId, teamId = part.TeamId, isCorrect = part.Correct, submittedAt = part.SubmittedAt });
        }

        private object? ParseOptions(string? options)
        {
            if (string.IsNullOrEmpty(options)) return null;
            try { return System.Text.Json.JsonSerializer.Deserialize<object>(options); } catch { return options; }
        }
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}
