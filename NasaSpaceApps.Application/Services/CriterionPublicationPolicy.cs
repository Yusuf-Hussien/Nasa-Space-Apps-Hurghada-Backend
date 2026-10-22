namespace NasaSpaceApps.Application.Services;

/// <summary>
/// Resolves which judges a publish / unpublish request applies to.
/// <para>
/// A <c>Judge</c> may only publish or withdraw <b>their own</b> scores.
/// An <c>Admin</c> may publish a specific judge (explicit id), every judge who
/// has scores on the criterion (force-publish), or withdraw the whole criterion.
/// </para>
/// </summary>
public static class CriterionPublicationPolicy
{
    public const string AdminRole = "Admin";
    public const string JudgeRole = "Judge";

    /// <summary>
    /// Determines the target judge ids for a publish/unpublish call.
    /// </summary>
    /// <param name="role">Role of the caller.</param>
    /// <param name="actorJudgeId">Judge profile linked to the caller, if any.</param>
    /// <param name="requestedJudgeId">Explicit target judge requested by an admin.</param>
    /// <param name="judgesWithScores">Judges who have scores on the criterion.</param>
    /// <returns>The judge ids affected, or an empty list when the call is not allowed.</returns>
    public static IReadOnlyList<int> ResolveTargets(
        string? role,
        int? actorJudgeId,
        int? requestedJudgeId,
        IReadOnlyCollection<int> judgesWithScores)
    {
        var isAdmin = string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase);
        var isJudge = string.Equals(role, JudgeRole, StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isJudge)
        {
            return Array.Empty<int>();
        }

        // A judge always acts on their own scores only.
        if (!isAdmin)
        {
            return actorJudgeId.HasValue ? new[] { actorJudgeId.Value } : Array.Empty<int>();
        }

        // Admin targeting one specific judge.
        if (requestedJudgeId.HasValue)
        {
            return new[] { requestedJudgeId.Value };
        }

        // Admin with no explicit target: applies to every judge who has scores.
        return judgesWithScores.Distinct().ToArray();
    }

    /// <summary>
    /// True when the caller may publish/unpublish at all (Judge or Admin only —
    /// CoreTeam cannot publish results).
    /// </summary>
    public static bool CanPublish(string? role) =>
        string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, JudgeRole, StringComparison.OrdinalIgnoreCase);
}
