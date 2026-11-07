using NasaSpaceApps.Application.Common;

namespace NasaSpaceApps.Application.Services;

public static class PasswordChangePolicy
{
    public const int MinimumLength = 6;

    public static AppError? Validate(
        string? currentPassword,
        string? newPassword,
        bool currentPasswordMatches)
    {
        if (!currentPasswordMatches)
        {
            return new AppError(
                "auth.invalid_current_password",
                400,
                "Current password is incorrect.");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < MinimumLength)
        {
            return new AppError(
                "auth.weak_password",
                400,
                $"New password must be at least {MinimumLength} characters.");
        }

        if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
        {
            return new AppError(
                "auth.password_unchanged",
                400,
                "New password must be different from the current password.");
        }

        return null;
    }
}
