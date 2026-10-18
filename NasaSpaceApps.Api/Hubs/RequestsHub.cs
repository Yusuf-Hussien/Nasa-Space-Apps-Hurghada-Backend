using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Api.Common;

namespace NasaSpaceApps.Api.Hubs;

/// <summary>Staff-only hub for the request queue. Organizers subscribe to
/// <see cref="OrganizerGroup"/> to receive request create/update/delete events.</summary>
[Authorize]
public class RequestsHub : Hub
{
    public const string Url = "/hubs/requests";
    public const string OrganizerGroup = "Organizers";

    public const string EventRequestCreated = "RequestCreated";
    public const string EventRequestUpdated = "RequestUpdated";
    public const string EventRequestDeleted = "RequestDeleted";

    public async Task JoinOrganizerGroup()
    {
        if (!Context.User.IsStaff())
        {
            throw new HubException("Only staff can join the organizer group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, OrganizerGroup);
    }

    public async Task LeaveOrganizerGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, OrganizerGroup);
    }
}
