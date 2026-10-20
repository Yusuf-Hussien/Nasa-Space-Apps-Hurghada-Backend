using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a staff account. Never exposes <c>PasswordHash</c>.</summary>
public sealed record StaffResponse(
    int StaffId,
    string FullName,
    string Username,
    string Email,
    string Role,
    string? MemberType,
    string? Photo,
    string Status,
    int? JudgeId,
    DateTime CreatedAt)
{
    public static StaffResponse From(Staff staff) =>
        new(
            StaffId: staff.StaffId,
            FullName: staff.FullName,
            Username: staff.Username,
            Email: staff.Email,
            Role: staff.Role,
            MemberType: staff.MemberType,
            Photo: staff.Photo,
            Status: staff.Status,
            JudgeId: staff.JudgeId,
            CreatedAt: staff.CreatedAt);
}
