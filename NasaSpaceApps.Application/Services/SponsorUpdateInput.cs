namespace NasaSpaceApps.Application.Services;

public sealed record SponsorUpdateInput(
    string Name,
    string? Logo,
    string? Link,
    int TierId,
    bool Active,
    int? DisplayOrder = null);
