namespace NasaSpaceApps.Application.Common;

/// <summary>
/// Validates base64-encoded images supplied by clients. Images are stored as
/// base64 text in the database, so there is no upload/serving pipeline - the
/// only guardrail is that the payload is a well-formed base64 image that decodes
/// to no more than <see cref="MaxBytes"/>.
/// </summary>
public static class ImageValidator
{
    /// <summary>20 MB, measured against the decoded byte length (not the base64 text).</summary>
    public const long MaxBytes = 20 * 1024 * 1024;

    private static readonly string[] AllowedMimeTypes =
    [
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/webp",
        "image/gif",
        "image/svg+xml"
    ];

    /// <summary>
    /// A stored image may be a base64 string, a data URI
    /// (<c>data:image/png;base64,...</c>) or (legacy) an absolute URL.
    /// URLs are passed through untouched for backwards compatibility.
    /// </summary>
    public static bool LooksLikeUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    public static string StripDataUriPrefix(string value)
    {
        var trimmed = value.Trim();
        if (!trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var commaIndex = trimmed.IndexOf(',');
        return commaIndex >= 0 ? trimmed[(commaIndex + 1)..] : trimmed;
    }

    /// <summary>
    /// The MIME type advertised by a data URI, or null when the value is plain
    /// base64 without a <c>data:</c> prefix.
    /// </summary>
    public static string? GetDataUriMimeType(string value)
    {
        var trimmed = value.Trim();
        if (!trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var semicolonIndex = trimmed.IndexOf(';');
        if (semicolonIndex < 0)
        {
            return null;
        }

        var mime = trimmed[5..semicolonIndex].Trim();
        return mime.Length == 0 ? null : mime;
    }

    /// <summary>
    /// Validates a base64 image value. Returns an error (code + message) when
    /// invalid, otherwise null. URLs are treated as valid (legacy values).
    /// </summary>
    public static ImageError? Validate(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || LooksLikeUrl(value))
        {
            return null;
        }

        var mime = GetDataUriMimeType(value);
        if (mime is not null && !AllowedMimeTypes.Contains(mime, StringComparer.OrdinalIgnoreCase))
        {
            return new ImageError(
                "image.invalid_type",
                $"{fieldName} must be one of: {string.Join(", ", AllowedMimeTypes)}.");
        }

        var base64 = StripDataUriPrefix(value).Trim();

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return new ImageError("image.invalid_base64", $"{fieldName} is not valid base64.");
        }

        if (bytes.LongLength > MaxBytes)
        {
            return new ImageError("image.too_large", $"{fieldName} exceeds the 20 MB limit after decoding.");
        }

        return null;
    }
}

/// <summary>Structured validation failure for an image field.</summary>
public sealed record ImageError(string Code, string Message);
