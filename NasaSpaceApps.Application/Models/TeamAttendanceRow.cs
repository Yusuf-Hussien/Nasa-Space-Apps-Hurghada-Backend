namespace NasaSpaceApps.Application.Models;

public sealed record TeamAttendanceRow(int TeamId, string TeamName, string Username, string Status, List<TeamAttendanceStudent> Students);
