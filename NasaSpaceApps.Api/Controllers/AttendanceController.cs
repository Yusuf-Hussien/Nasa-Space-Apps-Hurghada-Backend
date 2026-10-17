using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;

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
    public async Task<ActionResult> GetTeamsWithAttendance(
        [FromQuery] string? search = null,
        [FromQuery] DateTime? eventDate = null)
    {
        var result = await _attendanceService.GetTeamsWithAttendanceAsync(search, eventDate);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(row => new
        {
            teamId = row.TeamId,
            teamName = row.TeamName,
            username = row.Username,
            status = row.Status,
            students = row.Students.Select(s => new
            {
                studentId = s.StudentId,
                name = s.Name,
                email = s.Email,
                contact = s.Contact,
                attendanceStatus = s.AttendanceStatus,
                markedBy = s.MarkedBy
            })
        }));
    }

    /// <summary>
    /// Bulk mark / backfill attendance for an event date. Upserts per student+date
    /// so CoreTeam can mark others later or correct earlier marks.
    /// </summary>
    [Authorize(Policy = "CoreTeamOnly")]
    [HttpPost]
    public async Task<ActionResult> MarkBulk([FromBody] BulkAttendanceRequest request)
    {
        var result = await _attendanceService.MarkBulkAsync(
            request.EventDate,
            request.Entries.Select(e => new AttendanceEntry(e.StudentId, e.Status)).ToList(),
            _currentUser.Username ?? "unknown");

        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new
        {
            updated = result.Value!.Updated,
            created = result.Value.Created
        });
    }

    /// <summary>List attendance records with optional filters.</summary>
    [HttpGet]
    public async Task<ActionResult> GetAttendances(
        [FromQuery] int? teamId = null,
        [FromQuery] DateTime? eventDate = null,
        [FromQuery] string? status = null)
    {
        // Participants may only view their own team's attendance
        if (_currentUser.Role == AppRoles.Participant && _currentUser.TeamId.HasValue)
        {
            teamId = _currentUser.TeamId.Value;
        }

        var result = await _attendanceService.GetAttendancesAsync(teamId, eventDate, status);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value!.Select(a => new
        {
            attendanceId = a.AttendanceId,
            studentId = a.StudentId,
            studentName = a.Student?.Name,
            teamId = a.Student?.TeamId,
            eventDate = a.EventDate,
            status = a.Status,
            markedBy = a.MarkedBy,
            timestamp = a.Timestamp
        }));
    }
}
