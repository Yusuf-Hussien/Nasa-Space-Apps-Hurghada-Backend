using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NasaSpaceApps.Api.Hubs;

/// <summary>Public hub: anyone may listen to judge submissions and live standings.</summary>
[AllowAnonymous]
public class JudgingHub : Hub
{
    public const string Url = "/hubs/judging";

    public const string EventScoresSubmitted = "ScoresSubmitted";
    public const string EventStandingsUpdated = "StandingsUpdated";
}
