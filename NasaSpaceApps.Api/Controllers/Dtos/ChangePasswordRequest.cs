namespace NasaSpaceApps.Api.Controllers;

public sealed record ChangePasswordRequest(
    string CurrentPassword = "",
    string NewPassword = "");
