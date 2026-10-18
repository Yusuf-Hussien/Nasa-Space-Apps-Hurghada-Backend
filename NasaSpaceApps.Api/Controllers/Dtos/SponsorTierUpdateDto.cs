namespace NasaSpaceApps.Api.Controllers;

public sealed record SponsorTierUpdateDto(
    string Name,
    int? Priority = null,
    bool BannerEnabled = false);
