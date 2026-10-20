namespace NasaSpaceApps.Application.Models;

/// <summary>Create/update payload for a past-season media item.</summary>
public sealed record SeasonMediaInput(
    string Season,
    string Title,
    string? Description,
    string? Image,
    string? Link,
    string? MediaType,
    int? DisplayOrder = null,
    bool Active = true);
