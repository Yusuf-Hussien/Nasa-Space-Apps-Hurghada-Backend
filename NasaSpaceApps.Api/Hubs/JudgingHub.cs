using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NasaSpaceApps.Api.Hubs;

[AllowAnonymous]
public class JudgingHub : Hub
{
    public const string Url = "/hubs/judging";
}
