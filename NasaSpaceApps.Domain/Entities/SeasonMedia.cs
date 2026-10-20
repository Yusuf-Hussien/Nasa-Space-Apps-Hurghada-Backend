namespace NasaSpaceApps.Domain.Entities;

/// <summary>
/// A photo or footage item from a past season, shown on the public landing page.
/// <see cref="Image"/> is a base64-encoded image (or, for footage, a URL to the
/// media file); <see cref="Link"/> is an optional external link (recording/album).
/// </summary>
public class SeasonMedia
{
    public int MediaId { get; set; }
    public string Season { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Link { get; set; }
    public string? MediaType { get; set; }
    public int? DisplayOrder { get; set; }
    public bool Active { get; set; } = true;
}
