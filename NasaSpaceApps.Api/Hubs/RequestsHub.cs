using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Api.Common;

namespace NasaSpaceApps.Api.Hubs;

/// <summary>Hub for the request queue. Organizers subscribe to
/// <see cref="OrganizerGroup"/> and participants subscribe to their team group
/// to receive request create/update/delete events.</summary>
[Authorize]
public class RequestsHub : Hub
{
    public const string Url = "/hubs/requests";
    public const string OrganizerGroup = "Organizers";

    public const string EventRequestCreated = "RequestCreated";
    public const string EventRequestUpdated = "RequestUpdated";
    public const string EventRequestDeleted = "RequestDeleted";

    public static string TeamGroup(int teamId) => $"Team:{teamId}";

    public async Task JoinOrganizers()
    {
        if (!Context.User.IsStaff())
        {
            throw new HubException("Only staff can join the organizer group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, OrganizerGroup);
    }

    public Task JoinOrganizerGroup() => JoinOrganizers();

    public async Task LeaveOrganizers()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, OrganizerGroup);
    }

    public Task LeaveOrganizerGroup() => LeaveOrganizers();

    public async Task JoinTeamGroup()
    {
        var value = Context.User?.FindFirstValue("teamId");
        if (!int.TryParse(value, out var teamId))
        {
            throw new HubException("A team account is required to join a team group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, TeamGroup(teamId));
    }

    public async Task LeaveTeamGroup()
    {
        var value = Context.User?.FindFirstValue("teamId");
        if (int.TryParse(value, out var teamId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, TeamGroup(teamId));
        }
    }
}
