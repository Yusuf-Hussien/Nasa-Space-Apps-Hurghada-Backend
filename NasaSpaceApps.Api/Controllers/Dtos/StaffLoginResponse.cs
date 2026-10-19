using NasaSpaceApps.Application.Models;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// HTTP response shape for a successful staff login (Admin / CoreTeam / Judge).
/// Deliberately has no participant-only fields such as <c>teamId</c>.
/// </summary>
public sealed record StaffLoginResponse(
    string AccessToken,
    string Role,
    string Name,
    string Username,
    string Email,
    int StaffId,
    int? JudgeId)
{
    public static StaffLoginResponse From(LoginResponse value) =>
        new(
            AccessToken: value.AccessToken,
            Role: value.Role,
            Name: value.Name,
            Username: value.Username ?? string.Empty,
            Email: value.Email ?? string.Empty,
            StaffId: value.StaffId ?? 0,
            JudgeId: value.JudgeId);
}
