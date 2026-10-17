using Microsoft.AspNetCore.Mvc;
using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost]
    public async Task<ActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request)
    {
        var result = await _attendanceService.MarkAttendanceAsync(request.StudentId, request.EventDate, request.Status, request.MarkedBy);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.HttpStatus, new { code = result.Error.Code, message = result.Error.Message });
        }

        return Ok(new
        {
            attendanceId = result.Value!.AttendanceId,
            studentId = result.Value.StudentId,
            eventDate = result.Value.EventDate,
            status = result.Value.Status,
            markedBy = result.Value.MarkedBy,
            timestamp = result.Value.Timestamp
        });
    }
}

public sealed record MarkAttendanceRequest(int StudentId, DateTime EventDate, string Status, string? MarkedBy);
