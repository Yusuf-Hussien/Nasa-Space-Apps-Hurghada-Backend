namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// Payload for setting a single image. <c>image</c> is a base64 string
/// (optionally a full data URI the client already prefixed) or an absolute URL.
/// Set <c>image</c> to <c>null</c>/empty to clear it. Max 20 MB after decoding.
/// </summary>
public sealed record ImageInput(string? Image);
