using Microsoft.EntityFrameworkCore;
using NasaSpaceApps.Domain.Entities;
using NasaSpaceApps.Infrastructure.Services;

namespace NasaSpaceApps.Infrastructure.Data;

public class NasaSpaceAppsDbContext : DbContext
{
    public NasaSpaceAppsDbContext(DbContextOptions<NasaSpaceAppsDbContext> options) : base(options)
    {
    }

    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Judge> Judges => Set<Judge>();
    public DbSet<JudgingCriterion> JudgingCriteria => Set<JudgingCriterion>();
    public DbSet<JudgeScore> JudgeScores => Set<JudgeScore>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<GameParticipation> GameParticipations => Set<GameParticipation>();
    public DbSet<SponsorTier> SponsorTiers => Set<SponsorTier>();
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Challenge>().HasKey(challenge => challenge.ChallengeId);
        modelBuilder.Entity<Team>().HasKey(team => team.TeamId);
        modelBuilder.Entity<Student>().HasKey(student => student.StudentId);
        modelBuilder.Entity<Project>().HasKey(project => project.ProjectId);
        modelBuilder.Entity<Judge>().HasKey(judge => judge.JudgeId);
        modelBuilder.Entity<JudgingCriterion>().HasKey(criterion => criterion.CriterionId);
        modelBuilder.Entity<JudgeScore>().HasKey(score => score.ScoreId);
        modelBuilder.Entity<Attendance>().HasKey(attendance => attendance.AttendanceId);
        modelBuilder.Entity<ServiceRequest>().HasKey(request => request.RequestId);
        modelBuilder.Entity<Question>().HasKey(question => question.QuestionId);
        modelBuilder.Entity<GameParticipation>().HasKey(participation => participation.ParticipationId);
        modelBuilder.Entity<SponsorTier>().HasKey(tier => tier.TierId);
        modelBuilder.Entity<Sponsor>().HasKey(sponsor => sponsor.SponsorId);

        modelBuilder.Entity<Team>()
            .HasIndex(team => team.Username)
            .IsUnique();

        modelBuilder.Entity<Project>()
            .HasOne(project => project.Team)
            .WithOne(team => team.Project)
            .HasForeignKey<Project>(project => project.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
            .HasOne(student => student.Team)
            .WithMany(team => team.Students)
            .HasForeignKey(student => student.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JudgeScore>()
            .HasKey(score => score.ScoreId);

        modelBuilder.Entity<JudgeScore>()
            .Property(score => score.Score)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JudgingCriterion>()
            .Property(criterion => criterion.MaxScore)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JudgingCriterion>()
            .Property(criterion => criterion.Weight)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JudgeScore>()
            .HasOne(score => score.Team)
            .WithMany(team => team.Scores)
            .HasForeignKey(score => score.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JudgeScore>()
            .HasOne(score => score.Judge)
            .WithMany(judge => judge.Scores)
            .HasForeignKey(score => score.JudgeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JudgeScore>()
            .HasOne(score => score.Criterion)
            .WithMany(criterion => criterion.Scores)
            .HasForeignKey(score => score.CriterionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(request => request.Team)
            .WithMany(team => team.Requests)
            .HasForeignKey(request => request.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameParticipation>()
            .HasKey(participation => participation.ParticipationId);

        modelBuilder.Entity<GameParticipation>()
            .HasOne(participation => participation.Question)
            .WithMany(question => question.Participations)
            .HasForeignKey(participation => participation.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GameParticipation>()
            .HasOne(participation => participation.Team)
            .WithMany(team => team.GameParticipations)
            .HasForeignKey(participation => participation.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sponsor>()
            .HasOne(sponsor => sponsor.Tier)
            .WithMany(tier => tier.Sponsors)
            .HasForeignKey(sponsor => sponsor.TierId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}

public static class DbInitializer
{
    public static void Initialize(NasaSpaceAppsDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Challenges.Any())
        {
            return;
        }

        var challenge = new Challenge
        {
            Title = "Mars Habitat Challenge",
            Description = "Design sustainable life support and habitat operations for extravehicular systems.",
            Source = "NASA Space Apps",
            Season = "2026",
            Metadata = "{\"category\":\"Space Science\"}"
        };

        var passwordHasher = new PasswordHasherService();

        var team1 = new Team
        {
            Name = "Orion",
            Username = "orion",
            PasswordHash = passwordHasher.Hash("Password123!"),
            Challenge = challenge,
            Status = "Active"
        };

        var team2 = new Team
        {
            Name = "Atlas",
            Username = "atlas",
            PasswordHash = passwordHasher.Hash("Password123!"),
            Challenge = challenge,
            Status = "Active"
        };

        context.Challenges.Add(challenge);
        context.Teams.AddRange(team1, team2);

        var criteria = new[]
        {
            new JudgingCriterion { Name = "Innovation", MaxScore = 10m, Weight = 30m, Description = "Novelty and creativity." },
            new JudgingCriterion { Name = "Technical Execution", MaxScore = 10m, Weight = 40m, Description = "Depth of engineering implementation." },
            new JudgingCriterion { Name = "Impact", MaxScore = 10m, Weight = 30m, Description = "Real-world value and sustainability." }
        };

        context.JudgingCriteria.AddRange(criteria);

        var judge = new Judge { Name = "Dr. Ada Smith", Account = "ada.smith@spaceapps.org", Role = "Judge" };
        context.Judges.Add(judge);

        var project1 = new Project { Title = "Lunar Habitat Planner", Description = "Simulation planner", RepositoryLink = "https://example.com/repo1", DemoLink = "https://example.com/demo1", Team = team1 };
        var project2 = new Project { Title = "Mission Control Dashboard", Description = "Ops dashboard", RepositoryLink = "https://example.com/repo2", DemoLink = "https://example.com/demo2", Team = team2 };

        context.Projects.AddRange(project1, project2);

        var student1 = new Student { Name = "Aisha Ali", Team = team1, Email = "aisha@example.com", Contact = "555-1001", Status = "Active" };
        var student2 = new Student { Name = "Leo Park", Team = team1, Email = "leo@example.com", Contact = "555-1002", Status = "Active" };
        var student3 = new Student { Name = "Maya Chen", Team = team2, Email = "maya@example.com", Contact = "555-1003", Status = "Active" };
        context.Students.AddRange(student1, student2, student3);

        var tier = new SponsorTier { Name = "Tier 1", Priority = 1, BannerEnabled = true };
        context.SponsorTiers.Add(tier);
        context.Sponsors.Add(new Sponsor { Name = "NASA Partner", Tier = tier, Active = true, DisplayOrder = 1, Logo = "https://example.com/logo.png", Link = "https://example.com" });

        context.SaveChanges();

        var team1Score1 = new JudgeScore { TeamId = team1.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[0].CriterionId, Score = 9.5m, Comment = "Strong concept" };
        var team1Score2 = new JudgeScore { TeamId = team1.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[1].CriterionId, Score = 8.5m, Comment = "Solid execution" };
        var team2Score1 = new JudgeScore { TeamId = team2.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[0].CriterionId, Score = 9m, Comment = "Good idea" };
        var team2Score2 = new JudgeScore { TeamId = team2.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[1].CriterionId, Score = 7m, Comment = "Needs polish" };

        context.JudgeScores.AddRange(team1Score1, team1Score2, team2Score1, team2Score2);
        context.SaveChanges();
    }
}
