namespace NasaSpaceApps.Application.Services;

public sealed record TeamAttendanceStudent(int StudentId, string Name, string? Email, string? Contact, string? AttendanceStatus, string? MarkedBy);
