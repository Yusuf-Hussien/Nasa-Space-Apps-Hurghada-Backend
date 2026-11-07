using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Public HTTP response shape for an active CoreTeam member.
/// Deliberately excludes account and contact fields such as <c>email</c>,
/// <c>username</c>, <c>role</c>, <c>status</c>, and <c>passwordHash</c>.
/// </summary>
public sealed record CoreTeamMemberResponse(
    int StaffId,
    string FullName,
    string? MemberType,
    string? Photo)
{
    public static CoreTeamMemberResponse From(Staff staff) =>
        new(
            StaffId: staff.StaffId,
            FullName: staff.FullName,
            MemberType: staff.MemberType,
            Photo: staff.Photo);
}
