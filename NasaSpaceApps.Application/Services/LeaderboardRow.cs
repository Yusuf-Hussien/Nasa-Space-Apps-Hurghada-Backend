namespace NasaSpaceApps.Application.Services;

public sealed record LeaderboardRow(int Rank, int TeamId, string TeamName, int CorrectCount, int TotalPoints, bool IsTied);
