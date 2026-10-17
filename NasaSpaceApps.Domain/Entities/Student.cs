namespace NasaSpaceApps.Domain.Entities;

public class Student
{
    public int StudentId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Contact { get; set; }
    public string? Status { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
