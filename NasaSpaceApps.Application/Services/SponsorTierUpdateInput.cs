namespace NasaSpaceApps.Application.Services;

public sealed record SponsorTierUpdateInput(
    string Name,
    int? Priority = null,
    bool BannerEnabled = false);
