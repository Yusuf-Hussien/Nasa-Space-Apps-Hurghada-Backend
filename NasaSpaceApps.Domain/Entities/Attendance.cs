namespace NasaSpaceApps.Domain.Entities;

public class Attendance
{
    public int AttendanceId { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public DateTime EventDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MarkedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
