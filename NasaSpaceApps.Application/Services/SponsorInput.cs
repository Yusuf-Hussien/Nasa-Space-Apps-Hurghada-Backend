namespace NasaSpaceApps.Application.Services;

public sealed record SponsorInput(
    string Name,
    string? Logo,
    string? Link,
    int TierId,
    bool Active,
    int? DisplayOrder = null);
