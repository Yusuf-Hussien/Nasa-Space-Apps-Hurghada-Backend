namespace NasaSpaceApps.Api.Controllers;

public sealed record CreateRequestRequest(int TeamId, string Type, string Message);
