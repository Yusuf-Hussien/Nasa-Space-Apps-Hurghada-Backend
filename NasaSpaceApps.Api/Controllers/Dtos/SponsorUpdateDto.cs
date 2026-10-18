namespace NasaSpaceApps.Api.Controllers;

public sealed record SponsorUpdateDto(
    string Name,
    string? Logo,
    string? Link,
    int TierId,
    bool Active,
    int? DisplayOrder = null);
