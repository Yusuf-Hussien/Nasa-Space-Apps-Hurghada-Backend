namespace NasaSpaceApps.Domain.Entities;

public class Staff
{
    public int StaffId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? MemberType { get; set; }
    public string Status { get; set; } = "Active";
    public int? JudgeId { get; set; }
    public Judge? Judge { get; set; }
    public string? Photo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
