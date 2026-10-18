namespace NasaSpaceApps.Api.Controllers;

public sealed record SponsorTierInputDto(
    string Name,
    int? Priority = null,
    bool BannerEnabled = false);
