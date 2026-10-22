using Microsoft.EntityFrameworkCore;
using NasaSpaceApps.Domain.Entities;

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
    public DbSet<CriterionQuestion> CriterionQuestions => Set<CriterionQuestion>();
    public DbSet<QuestionScore> QuestionScores => Set<QuestionScore>();
    public DbSet<CriterionPublication> CriterionPublications => Set<CriterionPublication>();
    public DbSet<JudgeScore> JudgeScores => Set<JudgeScore>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<GameParticipation> GameParticipations => Set<GameParticipation>();
    public DbSet<SponsorTier> SponsorTiers => Set<SponsorTier>();
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<SeasonMedia> SeasonMedia => Set<SeasonMedia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Challenge>().HasKey(challenge => challenge.ChallengeId);
        modelBuilder.Entity<Team>().HasKey(team => team.TeamId);
        modelBuilder.Entity<Student>().HasKey(student => student.StudentId);
        modelBuilder.Entity<Project>().HasKey(project => project.ProjectId);
        modelBuilder.Entity<Judge>().HasKey(judge => judge.JudgeId);
        modelBuilder.Entity<JudgingCriterion>().HasKey(criterion => criterion.CriterionId);
        modelBuilder.Entity<CriterionQuestion>().HasKey(question => question.CriterionQuestionId);
        modelBuilder.Entity<QuestionScore>().HasKey(score => score.QuestionScoreId);
        modelBuilder.Entity<JudgeScore>().HasKey(score => score.ScoreId);
        modelBuilder.Entity<Attendance>().HasKey(attendance => attendance.AttendanceId);
        modelBuilder.Entity<ServiceRequest>().HasKey(request => request.RequestId);
        modelBuilder.Entity<Question>().HasKey(question => question.QuestionId);
        modelBuilder.Entity<GameParticipation>().HasKey(participation => participation.ParticipationId);
        modelBuilder.Entity<SponsorTier>().HasKey(tier => tier.TierId);
        modelBuilder.Entity<Sponsor>().HasKey(sponsor => sponsor.SponsorId);
        modelBuilder.Entity<Staff>().HasKey(staff => staff.StaffId);
        modelBuilder.Entity<SeasonMedia>().HasKey(media => media.MediaId);

        modelBuilder.Entity<Team>()
            .HasIndex(team => team.Username)
            .IsUnique();

        modelBuilder.Entity<Staff>()
            .HasIndex(staff => staff.Username)
            .IsUnique();

        modelBuilder.Entity<Staff>()
            .HasIndex(staff => staff.Email)
            .IsUnique();

        modelBuilder.Entity<Staff>()
            .HasOne(staff => staff.Judge)
            .WithMany()
            .HasForeignKey(staff => staff.JudgeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.StudentId, a.EventDate })
            .IsUnique();

        modelBuilder.Entity<GameParticipation>()
            .HasIndex(p => new { p.QuestionId, p.TeamId })
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
            .Property(score => score.Score)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JudgingCriterion>()
            .Property(criterion => criterion.MaxScore)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JudgingCriterion>()
            .Property(criterion => criterion.Weight)
            .HasPrecision(18, 2);

        modelBuilder.Entity<QuestionScore>()
            .Property(score => score.Score)
            .HasPrecision(18, 2);

        modelBuilder.Entity<QuestionScore>()
            .Property(score => score.MaxScore)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JudgingCriterion>()
            .HasOne(criterion => criterion.Challenge)
            .WithMany(challenge => challenge.Criteria)
            .HasForeignKey(criterion => criterion.ChallengeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CriterionQuestion>()
            .HasOne(question => question.Criterion)
            .WithMany(criterion => criterion.Questions)
            .HasForeignKey(question => question.CriterionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionScore>()
            .HasOne(score => score.Team)
            .WithMany()
            .HasForeignKey(score => score.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionScore>()
            .HasOne(score => score.Criterion)
            .WithMany()
            .HasForeignKey(score => score.CriterionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionScore>()
            .HasOne(score => score.CriterionQuestion)
            .WithMany()
            .HasForeignKey(score => score.CriterionQuestionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<QuestionScore>()
            .HasOne(score => score.Judge)
            .WithMany()
            .HasForeignKey(score => score.JudgeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CriterionPublication>()
            .HasOne(publication => publication.Criterion)
            .WithMany(criterion => criterion.Publications)
            .HasForeignKey(publication => publication.CriterionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CriterionPublication>()
            .HasOne(publication => publication.Judge)
            .WithMany()
            .HasForeignKey(publication => publication.JudgeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CriterionPublication>()
            .HasIndex(publication => new { publication.CriterionId, publication.JudgeId })
            .IsUnique();

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
