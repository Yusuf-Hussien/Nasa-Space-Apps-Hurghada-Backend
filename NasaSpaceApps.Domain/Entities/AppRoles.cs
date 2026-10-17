namespace NasaSpaceApps.Domain.Entities;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string CoreTeam = "CoreTeam";
    public const string Judge = "Judge";
    public const string Participant = "Participant";

    public static readonly IReadOnlyList<string> All = [Admin, CoreTeam, Judge, Participant];

    public static readonly IReadOnlyList<string> StaffRoles = [Admin, CoreTeam, Judge];

    public static bool IsStaff(string? role) =>
        role is not null && StaffRoles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public static bool IsValid(string? role) =>
        role is not null && All.Contains(role, StringComparer.OrdinalIgnoreCase);
}
