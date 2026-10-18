namespace NasaSpaceApps.Application.Models;

/// <summary>Shared create/update payload for a sponsor tier.</summary>
public sealed record SponsorTierInput(
    string Name,
    int? Priority = null,
    bool BannerEnabled = false);
