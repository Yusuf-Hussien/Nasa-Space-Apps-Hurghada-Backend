namespace NasaSpaceApps.Infrastructure.Data;

public class AppMigration
{
    public string Key { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
