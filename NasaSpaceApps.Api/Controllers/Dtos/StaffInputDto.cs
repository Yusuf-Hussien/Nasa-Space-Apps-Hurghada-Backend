namespace NasaSpaceApps.Api.Controllers;

public sealed record StaffInputDto(string FullName, string Username, string Email, string Password, string Role);
