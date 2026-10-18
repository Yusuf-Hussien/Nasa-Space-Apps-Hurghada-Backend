using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "StaffOnly")]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;
    private readonly ICurrentUser _currentUser;

    public AttendanceController(AttendanceService attendanceService, ICurrentUser currentUser)
    {
        _attendanceService = attendanceService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// CoreTeam check-in screen: all teams with per-student attendance for the given date.
    /// Search by teamId (numeric), name, or username.
    /// </summary>
    [Authorize(Policy = "CoreTeamOnly")]
    [HttpGet("teams")]
    public async Task<ActionResult<List<TeamAttendanceRow>>> GetTeamsWithAttendance(
        [FromQuery] string? search = null,
        [FromQuery] DateTime? eventDate = null)
    {
        var result = await _attendanceService.GetTeamsWithAttendanceAsync(search, eventDate);
        return result.ToHttpResult(this);
    }

    /// <summary>
    /// Bulk mark / backfill attendance for an event date. Upserts per student+date
    /// so CoreTeam can mark others later or correct earlier marks.
    /// </summary>
    [Authorize(Policy = "CoreTeamOnly")]
    [HttpPost]
    public async Task<ActionResult<BulkAttendanceResult>> MarkBulk([FromBody] BulkAttendanceRequest request)
    {
        var result = await _attendanceService.MarkBulkAsync(
            request.EventDate,
            request.Entries.Select(e => new AttendanceEntry(e.StudentId, e.Status)).ToList(),
            _currentUser.Username ?? "unknown");

        return result.ToHttpResult(this);
    }

    /// <summary>List attendance records with optional filters.</summary>
    [HttpGet]
    public async Task<ActionResult<List<AttendanceResponse>>> GetAttendances(
        [FromQuery] int? teamId = null,
        [FromQuery] DateTime? eventDate = null,
        [FromQuery] string? status = null)
    {
        var result = await _attendanceService.GetAttendancesAsync(teamId, eventDate, status);
        return result.ToHttpResult(this, rows => rows.Select(AttendanceResponse.From).ToList());
    }
}
