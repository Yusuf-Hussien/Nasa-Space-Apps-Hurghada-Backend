using System.Security.Claims;
using System.Text.Json;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Api.Controllers;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Tests;

public class GameAccessTests
{
    [Fact]
    public void IsAdmin_AdminRole_ReturnsTrue()
    {
        Assert.True(Principal(AppRoles.Admin).IsAdmin());
    }

    [Theory]
    [InlineData(AppRoles.CoreTeam)]
    [InlineData(AppRoles.Judge)]
    [InlineData(AppRoles.Participant)]
    public void IsAdmin_NonAdminRole_ReturnsFalse(string role)
    {
        Assert.False(Principal(role).IsAdmin());
    }

    [Fact]
    public void IsAdmin_UnauthenticatedPrincipal_ReturnsFalse()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Role, AppRoles.Admin)]));

        Assert.False(principal.IsAdmin());
    }

    [Fact]
    public void AnswerResponse_DoesNotExposeGrading()
    {
        var submittedAt = new DateTime(2026, 10, 4, 9, 30, 0, DateTimeKind.Utc);
        var response = AnswerResponse.From(new AnswerResult(
            Correct: true,
            Points: 10,
            SubmittedAt: submittedAt));

        var json = JsonSerializer.Serialize(response);

        Assert.Equal(submittedAt, response.SubmittedAt);
        Assert.DoesNotContain("correct", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("points", json, StringComparison.OrdinalIgnoreCase);
    }

    private static ClaimsPrincipal Principal(string role) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, role)],
            authenticationType: "Test"));
}
