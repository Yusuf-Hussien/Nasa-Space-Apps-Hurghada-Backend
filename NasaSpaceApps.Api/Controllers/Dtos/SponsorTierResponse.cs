using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a sponsor tier.</summary>
public sealed record SponsorTierResponse(
    int TierId,
    string Name,
    int? Priority,
    bool BannerEnabled)
{
    public static SponsorTierResponse From(SponsorTier tier) =>
        new(tier.TierId, tier.Name, tier.Priority, tier.BannerEnabled);
}
