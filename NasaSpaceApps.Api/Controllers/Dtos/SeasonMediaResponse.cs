using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>HTTP response shape for a past-season photo/footage item.</summary>
public sealed record SeasonMediaResponse(
    int MediaId,
    string Season,
    string Title,
    string? Description,
    string? Image,
    string? Link,
    string? MediaType,
    int? DisplayOrder,
    bool Active)
{
    public static SeasonMediaResponse From(SeasonMedia media) =>
        new(
            media.MediaId,
            media.Season,
            media.Title,
            media.Description,
            media.Image,
            media.Link,
            media.MediaType,
            media.DisplayOrder,
            media.Active);
}
