namespace NasaSpaceApps.Application.Models;

/// <summary>Shared create/update payload for a sponsor.</summary>
public sealed record SponsorInput(
    string Name,
    string? Logo,
    string? Link,
    int TierId,
    bool Active,
    int? DisplayOrder = null);
