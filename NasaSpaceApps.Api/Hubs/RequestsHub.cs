using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Domain.Entities;

namespace NasaSpaceApps.Api.Hubs;

[Authorize]
public class RequestsHub : Hub
{
    public const string Url = "/hubs/requests";

    public async Task JoinOrganizerGroup()
    {
        if (Context.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var isStaff = Context.User.IsInRole(AppRoles.Admin) ||
                      Context.User.IsInRole(AppRoles.CoreTeam) ||
                      Context.User.IsInRole(AppRoles.Judge);

        if (!isStaff)
        {
            throw new HubException("Only staff can join the organizer group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, "Organizers");
    }

    public async Task LeaveOrganizerGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Organizers");
    }
}

