using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NasaSpaceApps.Api.Common;

namespace NasaSpaceApps.Api.Hubs;

/// <summary>Hub for live quiz-game events. Only admins may subscribe to the
/// admin group; participants subscribe to the participant group with their team
/// token so question releases and game state changes can be pushed safely.</summary>
[Authorize]
public class GameHub : Hub
{
    public const string Url = "/hubs/game";
    public const string AdminsGroup = "GameAdmins";
    public const string ParticipantsGroup = "GameParticipants";

    public const string EventGameStateChanged = "GameStateChanged";
    public const string EventQuestionReleased = "QuestionReleased";
    public const string EventQuestionUpdated = "QuestionUpdated";
    public const string EventQuestionRetracted = "QuestionRetracted";
    public const string EventAnswerSubmitted = "AnswerSubmitted";
    public const string EventLeaderboardUpdated = "LeaderboardUpdated";

    public async Task JoinAdminGroup()
    {
        if (!Context.User.IsAdmin())
        {
            throw new HubException("Only admins can join the game admin group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminsGroup);
    }

    public async Task JoinParticipantGroup()
    {
        var value = Context.User?.FindFirstValue("teamId");
        if (!int.TryParse(value, out _))
        {
            throw new HubException("A team account is required to join the game participant group.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ParticipantsGroup);
    }

    public async Task LeaveParticipantGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ParticipantsGroup);
    }
}

public sealed record GameAnswerSubmittedEvent(
    int QuestionId,
    int TeamId,
    string Answer,
    bool Correct,
    int Points,
    DateTime SubmittedAt);

public sealed record GameQuestionRetractedEvent(int QuestionId);
