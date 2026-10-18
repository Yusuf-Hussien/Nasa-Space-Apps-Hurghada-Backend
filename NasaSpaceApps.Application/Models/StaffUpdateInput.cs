namespace NasaSpaceApps.Application.Models;

/// <summary>Update payload for a staff account. <see cref="Status"/> is only settable here.</summary>
public sealed record StaffUpdateInput(
    string FullName,
    string Username,
    string Email,
    string? Password,
    string Role,
    string? MemberType = null,
    string? Status = null);
