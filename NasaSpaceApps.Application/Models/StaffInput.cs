namespace NasaSpaceApps.Application.Models;

/// <summary>Create payload for a staff account.</summary>
public sealed record StaffInput(
    string FullName,
    string Username,
    string Email,
    string? Password,
    string Role,
    string? MemberType = null);
