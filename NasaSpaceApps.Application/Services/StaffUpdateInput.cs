namespace NasaSpaceApps.Application.Services;

public sealed record StaffUpdateInput(
    string FullName,
    string Username,
    string Email,
    string? Password,
    string Role,
    string? MemberType = null,
    string? Status = null);
