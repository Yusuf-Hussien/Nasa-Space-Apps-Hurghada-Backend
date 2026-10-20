using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// HTTP response shape for a successful participant login.
/// Carries the team's own details, its challenge, and its roster so the mobile
/// app can render the dashboard without extra round-trips.
/// Deliberately has no staff-only fields such as <c>email</c>, <c>staffId</c> or <c>judgeId</c>.
/// </summary>
public sealed record ParticipantLoginResponse(
    string AccessToken,
    string Role,
    int TeamId,
    ParticipantTeamResponse Team,
    ParticipantChallengeResponse Challenge,
    List<StudentResponse> Students)
{
    public static ParticipantLoginResponse From(
        string accessToken,
        Team team,
        IReadOnlyCollection<Student> students) =>
        new(
            AccessToken: accessToken,
            Role: AppRoles.Participant,
            TeamId: team.TeamId,
            Team: ParticipantTeamResponse.From(team),
            Challenge: team.Challenge is null
                ? ParticipantChallengeResponse.Empty
                : ParticipantChallengeResponse.From(team.Challenge),
            Students: students.Select(StudentResponse.From).ToList());
}

/// <summary>The logged-in team's own record.</summary>
public sealed record ParticipantTeamResponse(
    int TeamId,
    string Name,
    string Username,
    string? Logo,
    string Status)
{
    public static ParticipantTeamResponse From(Team team) =>
        new(team.TeamId, team.Name, team.Username, team.Logo, team.Status);
}

/// <summary>The challenge assigned to the logged-in team.</summary>
public sealed record ParticipantChallengeResponse(
    int ChallengeId,
    string Title,
    string? Description,
    string? Source,
    string? Season,
    string? Image)
{
    public static readonly ParticipantChallengeResponse Empty = new(0, string.Empty, null, null, null, null);

    public static ParticipantChallengeResponse From(Challenge challenge) =>
        new(challenge.ChallengeId, challenge.Title, challenge.Description, challenge.Source, challenge.Season, challenge.Image);
}
