namespace NasaSpaceApps.Domain.Entities;

public class GameState
{
    public const int SingletonId = 1;

    public int GameStateId { get; set; } = SingletonId;
    public bool IsStarted { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
