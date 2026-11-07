using NasaSpaceApps.Api.Controllers;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Tests;

public class CoreTeamMemberResponseTests
{
    [Fact]
    public void From_Should_Map_Public_Fields()
    {
        var staff = new Staff
        {
            StaffId = 7,
            FullName = "Sara Hassan",
            Username = "sara.mentor",
            Email = "sara.mentor@nasa.local",
            PasswordHash = "secret",
            Role = AppRoles.CoreTeam,
            MemberType = CoreTeamTypes.Mentor,
            Status = "Active",
            JudgeId = 3,
            Photo = "data:image/png;base64,abc"
        };

        var response = CoreTeamMemberResponse.From(staff);

        Assert.Equal(7, response.StaffId);
        Assert.Equal("Sara Hassan", response.FullName);
        Assert.Equal(CoreTeamTypes.Mentor, response.MemberType);
        Assert.Equal("data:image/png;base64,abc", response.Photo);
    }

    [Fact]
    public void Response_Should_Expose_Only_Public_Properties()
    {
        var propertyNames = typeof(CoreTeamMemberResponse)
            .GetProperties()
            .Select(p => p.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            new[] { "FullName", "MemberType", "Photo", "StaffId" },
            propertyNames);
    }
}
