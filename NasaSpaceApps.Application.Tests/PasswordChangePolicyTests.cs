using NasaSpaceApps.Application.Services;

namespace NasaSpaceApps.Application.Tests;

public class PasswordChangePolicyTests
{
    [Fact]
    public void Validate_ValidChange_ReturnsNull()
    {
        var error = PasswordChangePolicy.Validate("OldPass1!", "NewPass1!", currentPasswordMatches: true);

        Assert.Null(error);
    }

    [Fact]
    public void Validate_WrongCurrentPassword_ReturnsInvalidCurrentPassword()
    {
        var error = PasswordChangePolicy.Validate("wrong", "NewPass1!", currentPasswordMatches: false);

        Assert.NotNull(error);
        Assert.Equal("auth.invalid_current_password", error.Code);
        Assert.Equal(400, error.HttpStatus);
    }

    [Fact]
    public void Validate_NewPasswordTooShort_ReturnsWeakPassword()
    {
        var error = PasswordChangePolicy.Validate("OldPass1!", "12345", currentPasswordMatches: true);

        Assert.NotNull(error);
        Assert.Equal("auth.weak_password", error.Code);
        Assert.Equal(400, error.HttpStatus);
    }

    [Fact]
    public void Validate_NewPasswordWhitespace_ReturnsWeakPassword()
    {
        var error = PasswordChangePolicy.Validate("OldPass1!", "      ", currentPasswordMatches: true);

        Assert.NotNull(error);
        Assert.Equal("auth.weak_password", error.Code);
    }

    [Fact]
    public void Validate_NewPasswordSameAsCurrent_ReturnsPasswordUnchanged()
    {
        var error = PasswordChangePolicy.Validate("SamePass1!", "SamePass1!", currentPasswordMatches: true);

        Assert.NotNull(error);
        Assert.Equal("auth.password_unchanged", error.Code);
        Assert.Equal(400, error.HttpStatus);
    }

    [Fact]
    public void Validate_WrongCurrentTakesPrecedenceOverWeakNewPassword()
    {
        var error = PasswordChangePolicy.Validate("wrong", "123", currentPasswordMatches: false);

        Assert.NotNull(error);
        Assert.Equal("auth.invalid_current_password", error.Code);
    }
}
