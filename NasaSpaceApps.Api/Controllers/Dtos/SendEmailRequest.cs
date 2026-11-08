namespace NasaSpaceApps.Api.Controllers;

/// <summary>Admin bulk-email request. Recipients are sent individually and deduplicated.</summary>
public sealed record SendEmailRequest(
    string Subject = "",
    string Body = "",
    List<string>? Recipients = null,
    bool IsBodyHtml = false);
