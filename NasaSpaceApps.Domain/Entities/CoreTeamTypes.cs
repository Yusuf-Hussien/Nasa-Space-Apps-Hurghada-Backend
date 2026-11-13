namespace NasaSpaceApps.Domain.Entities;

public static class CoreTeamTypes
{
    public const string LocalLead = "Local Lead";
    public const string Highboard = "Highboard";
    public const string Mentor = "Mentor";
    public const string Media = "Media";
    public const string Marketing = "Marketing";
    public const string PR = "PR";
    public const string Organizer = "Organizer";
    public const string Developer = "Developer";

    public static readonly IReadOnlyList<string> All =
        [LocalLead, Highboard, Developer, Mentor, Media, Marketing, PR, Organizer];

    private static readonly IReadOnlyDictionary<string, int> SortRanks =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [LocalLead] = 0,
            [Highboard] = 1,
            [Developer] = 2,
            [Mentor] = 3,
            [Media] = 4,
            [Marketing] = 5,
            [PR] = 6,
            [Organizer] = 7,
        };

    public static bool IsValid(string? type) =>
        type is not null && All.Contains(type, StringComparer.OrdinalIgnoreCase);

    public static int Rank(string? type) =>
        type is not null && SortRanks.TryGetValue(type.Trim(), out var rank)
            ? rank
            : int.MaxValue;

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
