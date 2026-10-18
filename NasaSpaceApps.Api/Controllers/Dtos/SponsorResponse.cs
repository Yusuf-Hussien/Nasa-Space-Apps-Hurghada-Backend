using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a sponsor.</summary>
public sealed record SponsorResponse(
    int SponsorId,
    string Name,
    string? Logo,
    string? Link,
    int TierId,
    SponsorTierResponse? Tier,
    bool Active,
    int? DisplayOrder)
{
    public static SponsorResponse From(Sponsor sponsor, bool includeTier = true) =>
        new(
            SponsorId: sponsor.SponsorId,
            Name: sponsor.Name,
            Logo: sponsor.Logo,
            Link: sponsor.Link,
            TierId: sponsor.TierId,
            Tier: includeTier && sponsor.Tier is not null ? SponsorTierResponse.From(sponsor.Tier) : null,
            Active: sponsor.Active,
            DisplayOrder: sponsor.DisplayOrder);
}
