using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Controllers;

/// <summary>
/// HTTP response shape for a single team. Public view: sensitive fields
/// (<c>username</c>, student email/contact) are stripped for non-staff callers.
/// </summary>
public sealed record TeamDetailResponse(
    int TeamId,
    string Name,
    string? Username,
    string? Logo,
    string Status,
    ChallengeResponse? Challenge,
    ProjectResponse? Project,
    List<TeamMemberResponse> Students)
{
    public static TeamDetailResponse From(Team team, bool includeSensitive = false) =>
        new(
            TeamId: team.TeamId,
            Name: team.Name,
            Username: includeSensitive ? team.Username : null,
            Logo: team.Logo,
            Status: team.Status,
            Challenge: team.Challenge is null ? null : ChallengeResponse.From(team.Challenge),
            Project: team.Project is null ? null : ProjectResponse.From(team.Project),
            Students: team.Students
                .OrderBy(student => student.Name)
                .Select(TeamMemberResponse.From)
                .ToList());
}

/// <summary>A student on the team — names only, no contact details.</summary>
public sealed record TeamMemberResponse(int StudentId, string Name, string? Status)
{
    public static TeamMemberResponse From(Student student) =>
        new(student.StudentId, student.Name, student.Status);
}

/// <summary>A team's project submission.</summary>
public sealed record ProjectResponse(
    string Title,
    string? Description,
    string? RepositoryLink,
    string? DemoLink)
{
    public static ProjectResponse From(Project project) =>
        new(project.Title, project.Description, project.RepositoryLink, project.DemoLink);
}
