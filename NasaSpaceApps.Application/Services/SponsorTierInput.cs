namespace NasaSpaceApps.Application.Services;

public sealed record SponsorTierInput(
    string Name,
    int? Priority = null,
    bool BannerEnabled = false);
