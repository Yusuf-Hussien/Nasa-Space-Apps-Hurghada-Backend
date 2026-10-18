using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for an attendance record.</summary>
public sealed record AttendanceResponse(
    int AttendanceId,
    int StudentId,
    string? StudentName,
    int? TeamId,
    DateTime EventDate,
    string Status,
    string? MarkedBy,
    DateTime Timestamp)
{
    public static AttendanceResponse From(Attendance attendance) =>
        new(
            AttendanceId: attendance.AttendanceId,
            StudentId: attendance.StudentId,
            StudentName: attendance.Student?.Name,
            TeamId: attendance.Student?.TeamId,
            EventDate: attendance.EventDate,
            Status: attendance.Status,
            MarkedBy: attendance.MarkedBy,
            Timestamp: attendance.Timestamp);
}
