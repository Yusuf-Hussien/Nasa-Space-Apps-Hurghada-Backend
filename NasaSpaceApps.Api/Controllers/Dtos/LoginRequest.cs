namespace NasaSpaceApps.Api.Controllers;

public sealed record LoginRequest(string? Identifier, int? TeamId, string Password);
