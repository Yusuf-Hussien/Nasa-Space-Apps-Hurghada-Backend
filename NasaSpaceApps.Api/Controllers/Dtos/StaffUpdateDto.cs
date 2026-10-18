namespace NasaSpaceApps.Api.Controllers;

public sealed record StaffUpdateDto(
    string FullName,
    string Username,
    string Email,
    string? Password,
    string Role,
    string? MemberType = null,
    string? Status = null);
