using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Infrastructure.Data;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StandingsController : ControllerBase
{
    private readonly NasaSpaceAppsDbContext _context;

    public StandingsController(NasaSpaceAppsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetStandings()
    {
        var teams = await _context.Teams.OrderBy(team => team.Name).ToListAsync();
        var scores = await _context.JudgeScores.ToListAsync();
        var standings = StandingsCalculator.Calculate(scores, teams);

        return Ok(new
        {
            version = standings.Count == 0 ? 0 : standings.Max(item => item.Rank ?? 0) + 1,
            updatedAt = DateTime.UtcNow,
            items = standings.Select(item => new
            {
                rank = item.Rank,
                teamId = item.TeamId,
                teamName = item.TeamName,
                totalScore = item.TotalScore,
                isTied = item.IsTied
            })
        });
    }
}
