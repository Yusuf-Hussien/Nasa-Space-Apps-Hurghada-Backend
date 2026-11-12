namespace NasaSpaceApps.Application.Services;

/// <summary>
/// Resolves which judge/team score pairs a publish / unpublish request applies to.
/// <para>
/// A <c>Judge</c> may only publish or withdraw <b>their own</b> scores.
/// An <c>Admin</c> may publish a specific judge and/or team, every scored pair
/// on the criterion (force-publish), or withdraw the whole criterion.
/// </para>
/// </summary>
public static class CriterionPublicationPolicy
{
    public const string AdminRole = "Admin";
    public const string JudgeRole = "Judge";

    /// <summary>
    /// Determines the target judge/team pairs for a publish/unpublish call.
    /// </summary>
    /// <param name="role">Role of the caller.</param>
    /// <param name="actorJudgeId">Judge profile linked to the caller, if any.</param>
    /// <param name="requestedJudgeId">Explicit target judge requested by an admin.</param>
    /// <param name="requestedTeamId">Explicit target team requested by an admin or judge.</param>
    /// <param name="scoredTargets">Judge/team pairs that have scores on the criterion.</param>
    /// <returns>The pairs affected, or an empty list when the call is not allowed.</returns>
    public static IReadOnlyList<(int JudgeId, int TeamId)> ResolveTargets(
        string? role,
        int? actorJudgeId,
        int? requestedJudgeId,
        int? requestedTeamId,
        IReadOnlyCollection<(int JudgeId, int TeamId)> scoredTargets)
    {
        var isAdmin = string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase);
        var isJudge = string.Equals(role, JudgeRole, StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isJudge)
        {
            return Array.Empty<(int JudgeId, int TeamId)>();
        }

        var query = scoredTargets.AsEnumerable();

        // A judge always acts on their own scores only.
        if (!isAdmin)
        {
            if (!actorJudgeId.HasValue)
            {
                return Array.Empty<(int JudgeId, int TeamId)>();
            }

            query = query.Where(target => target.JudgeId == actorJudgeId.Value);
        }
        else if (requestedJudgeId.HasValue)
        {
            query = query.Where(target => target.JudgeId == requestedJudgeId.Value);
        }

        if (requestedTeamId.HasValue)
        {
            query = query.Where(target => target.TeamId == requestedTeamId.Value);
        }

        return query.Distinct().ToArray();
    }

    /// <summary>
    /// True when the caller may publish/unpublish at all (Judge or Admin only —
    /// CoreTeam cannot publish results).
    /// </summary>
    public static bool CanPublish(string? role) =>
        string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, JudgeRole, StringComparison.OrdinalIgnoreCase);
}
