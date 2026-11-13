using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Tests;

public class CoreTeamTypesTests
{
    [Fact]
    public void Rank_Should_Order_Roles_By_Priority()
    {
        var roles = new[]
        {
            CoreTeamTypes.Organizer,
            CoreTeamTypes.PR,
            CoreTeamTypes.Marketing,
            CoreTeamTypes.Media,
            CoreTeamTypes.Mentor,
            CoreTeamTypes.Developer,
            CoreTeamTypes.Highboard,
            CoreTeamTypes.LocalLead
        };

        var ordered = roles.OrderBy(CoreTeamTypes.Rank).ToArray();

        Assert.Equal(
            new[]
            {
                CoreTeamTypes.LocalLead,
                CoreTeamTypes.Highboard,
                CoreTeamTypes.Developer,
                CoreTeamTypes.Mentor,
                CoreTeamTypes.Media,
                CoreTeamTypes.Marketing,
                CoreTeamTypes.PR,
                CoreTeamTypes.Organizer
            },
            ordered);
    }

    [Fact]
    public void Rank_Should_Place_Developer_Before_Mentor()
    {
        Assert.True(
            CoreTeamTypes.Rank(CoreTeamTypes.Developer) <
            CoreTeamTypes.Rank(CoreTeamTypes.Mentor));
    }

    [Fact]
    public void Rank_Should_Put_Unknown_Or_Missing_Types_Last()
    {
        Assert.Equal(int.MaxValue, CoreTeamTypes.Rank("Unknown"));
        Assert.Equal(int.MaxValue, CoreTeamTypes.Rank(null));
    }

    [Fact]
    public void All_Should_Include_Local_Lead()
    {
        Assert.Contains(CoreTeamTypes.LocalLead, CoreTeamTypes.All);
    }
}
