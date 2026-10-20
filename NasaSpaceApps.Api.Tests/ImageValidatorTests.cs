using NasaSpaceApps.Application.Common;

namespace NasaSpaceApps.Api.Tests;

public class ImageValidatorTests
{
    private static string Base64Of(long byteCount) =>
        Convert.ToBase64String(new byte[byteCount]);

    [Fact]
    public void Validate_Should_Allow_NullOrEmpty()
    {
        Assert.Null(ImageValidator.Validate(null, "logo"));
        Assert.Null(ImageValidator.Validate("", "logo"));
        Assert.Null(ImageValidator.Validate("   ", "logo"));
    }

    [Fact]
    public void Validate_Should_Allow_LegacyUrl()
    {
        Assert.Null(ImageValidator.Validate("https://example.com/logo.png", "logo"));
    }

    [Fact]
    public void Validate_Should_Allow_PlainBase64_UnderLimit()
    {
        Assert.Null(ImageValidator.Validate(Base64Of(1024), "logo"));
    }

    [Fact]
    public void Validate_Should_Allow_DataUri_UnderLimit()
    {
        var dataUri = $"data:image/png;base64,{Base64Of(1024)}";
        Assert.Null(ImageValidator.Validate(dataUri, "logo"));
    }

    [Fact]
    public void Validate_Should_Reject_InvalidBase64()
    {
        var error = ImageValidator.Validate("not-valid-base64!!!", "logo");

        Assert.NotNull(error);
        Assert.Equal("image.invalid_base64", error!.Code);
    }

    [Fact]
    public void Validate_Should_Reject_WhenOver20Mb_AfterDecoding()
    {
        var dataUri = $"data:image/png;base64,{Base64Of(ImageValidator.MaxBytes + 1)}";
        var error = ImageValidator.Validate(dataUri, "logo");

        Assert.NotNull(error);
        Assert.Equal("image.too_large", error!.Code);
    }

    [Fact]
    public void Validate_Should_Allow_Exactly20Mb()
    {
        var dataUri = $"data:image/png;base64,{Base64Of(ImageValidator.MaxBytes)}";
        Assert.Null(ImageValidator.Validate(dataUri, "logo"));
    }

    [Fact]
    public void Validate_Should_Reject_DisallowedDataUriMimeType()
    {
        var dataUri = $"data:application/pdf;base64,{Base64Of(16)}";
        var error = ImageValidator.Validate(dataUri, "logo");

        Assert.NotNull(error);
        Assert.Equal("image.invalid_type", error!.Code);
    }

    [Fact]
    public void StripDataUriPrefix_Should_ReturnPayload()
    {
        var payload = Base64Of(16);
        Assert.Equal(payload, ImageValidator.StripDataUriPrefix($"data:image/png;base64,{payload}"));
        Assert.Equal(payload, ImageValidator.StripDataUriPrefix(payload));
    }
}
