namespace NasaSpaceApps.Domain.Entities;

public static class CoreTeamTypes
{
    public const string Organizer = "Organizer";
    public const string Mentor = "Mentor";
    public const string Developer = "Developer";
    public const string Marketing = "Marketing";
    public const string PR = "PR";
    public const string Media = "Media";
    public const string Highboard = "Highboard";

    public static readonly IReadOnlyList<string> All =
        [Organizer, Mentor, Developer, Marketing, PR, Media, Highboard];

    public static bool IsValid(string? type) =>
        type is not null && All.Contains(type, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string type)
    {
        foreach (var known in All)
        {
            if (string.Equals(type.Trim(), known, StringComparison.OrdinalIgnoreCase))
            {
                return known;
            }
        }

        return type.Trim();
    }
}
