namespace NasaSpaceApps.Domain.Entities;

public class Challenge
{
    public int ChallengeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Source { get; set; }
    public string? Season { get; set; }
    public string? Metadata { get; set; }
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class Team
{
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    public string Status { get; set; } = "Active";
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public Project? Project { get; set; }
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
    public ICollection<ServiceRequest> Requests { get; set; } = new List<ServiceRequest>();
    public ICollection<GameParticipation> GameParticipations { get; set; } = new List<GameParticipation>();
}

public class Student
{
    public int StudentId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Contact { get; set; }
    public string? Status { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}

public class Project
{
    public int ProjectId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RepositoryLink { get; set; }
    public string? DemoLink { get; set; }
}

public class Judge
{
    public int JudgeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Account { get; set; }
    public string? Role { get; set; }
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
}

public class JudgingCriterion
{
    public int CriterionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public ICollection<JudgeScore> Scores { get; set; } = new List<JudgeScore>();
}

public class JudgeScore
{
    public int ScoreId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int JudgeId { get; set; }
    public Judge? Judge { get; set; }
    public int CriterionId { get; set; }
    public JudgingCriterion? Criterion { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class Attendance
{
    public int AttendanceId { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public DateTime EventDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MarkedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ServiceRequest
{
    public int RequestId { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

public class Question
{
    public int QuestionId { get; set; }
    public int ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Options { get; set; }
    public string? CorrectAnswer { get; set; }
    public ICollection<GameParticipation> Participations { get; set; } = new List<GameParticipation>();
}

public class GameParticipation
{
    public int ParticipationId { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public string? Answer { get; set; }
    public bool Correct { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class SponsorTier
{
    public int TierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Priority { get; set; }
    public bool BannerEnabled { get; set; }
    public ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();
}

public class Sponsor
{
    public int SponsorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Link { get; set; }
    public int TierId { get; set; }
    public SponsorTier? Tier { get; set; }
    public bool Active { get; set; }
    public int? DisplayOrder { get; set; }
}
