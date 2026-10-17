namespace NasaSpaceApps.Domain.Entities;

public static class AttendanceStatuses
{
    public const string Present = "Present";
    public const string Absent = "Absent";
    public const string Late = "Late";

    public static readonly IReadOnlyList<string> All = [Present, Absent, Late];

    public static bool IsValid(string? status) =>
        status is not null && All.Contains(status, StringComparer.OrdinalIgnoreCase);
}
