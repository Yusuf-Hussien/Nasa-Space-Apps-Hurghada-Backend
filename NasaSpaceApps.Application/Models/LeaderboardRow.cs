namespace NasaSpaceApps.Application.Models;

public sealed record LeaderboardRow(int Rank, int TeamId, string TeamName, int CorrectCount, int TotalPoints, bool IsTied);
