using NasaSpaceApps.Api.Controllers;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Tests;

public class TeamDetailResponseTests
{
    private static Team BuildTeam() => new()
    {
        TeamId = 1,
        Name = "Orion",
        Username = "orion",
        Status = "Active",
        Logo = null,
        Challenge = new Challenge { ChallengeId = 7, Title = "Mars Habitat", Season = "2026" },
        Project = new Project { Title = "Lunar Planner", Description = "Sim", RepositoryLink = "https://repo", DemoLink = "https://demo" },
        Students = new List<Student>
        {
            new() { StudentId = 1, Name = "Aisha Ali", Email = "aisha@example.com", Contact = "555-1001", Status = "Active" },
            new() { StudentId = 2, Name = "Leo Park", Email = "leo@example.com", Contact = "555-1002", Status = "Active" }
        }
    };

    [Fact]
    public void From_PublicCaller_Should_Hide_Username_And_StudentContactDetails()
    {
        var response = TeamDetailResponse.From(BuildTeam(), includeSensitive: false);

        Assert.Null(response.Username);
        Assert.Equal(7, response.Challenge!.ChallengeId);
        Assert.Equal("Lunar Planner", response.Project!.Title);
        Assert.Equal(2, response.Students.Count);
        Assert.All(response.Students, member => Assert.False(string.IsNullOrWhiteSpace(member.Name)));
    }

    [Fact]
    public void From_StaffCaller_Should_Include_Username()
    {
        var response = TeamDetailResponse.From(BuildTeam(), includeSensitive: true);

        Assert.Equal("orion", response.Username);
    }

    [Fact]
    public void TeamMemberResponse_Should_Expose_Only_Name_And_Status()
    {
        var member = TeamMemberResponse.From(new Student
        {
            StudentId = 3,
            Name = "Maya",
            Email = "maya@example.com",
            Contact = "555-1003",
            Status = "Active"
        });

        Assert.Equal("Maya", member.Name);
        Assert.Equal("Active", member.Status);
    }
}
