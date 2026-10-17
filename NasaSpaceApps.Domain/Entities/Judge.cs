namespace NasaSpaceApps.Domain.Entities;

public class Judge
{
    public int JudgeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Account { get; set; }
    public string? Role { get; set; }
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
}
