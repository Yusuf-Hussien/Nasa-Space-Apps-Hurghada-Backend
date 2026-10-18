namespace NasaSpaceApps.Api.Controllers;

public sealed record SponsorInputDto(
    string Name,
    string? Logo,
    string? Link,
    int TierId,
    bool Active,
    int? DisplayOrder = null);
