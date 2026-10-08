using Microsoft.EntityFrameworkCore;
using NASAHackathon.API.Models;

namespace NASAHackathon.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Judge> Judges { get; set; }
        public DbSet<JudgingCriterion> JudgingCriteria { get; set; }
        public DbSet<JudgeScore> JudgeScores { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<GameParticipation> GameParticipations { get; set; }
        public DbSet<SponsorTier> SponsorTiers { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Team>()
                .HasIndex(t => t.Username)
                .IsUnique();

            modelBuilder.Entity<Project>()
                .HasIndex(p => p.TeamId)
                .IsUnique();

            modelBuilder.Entity<JudgeScore>()
                .HasIndex(js => new { js.JudgeId, js.TeamId, js.CriterionId })
                .IsUnique();

            modelBuilder.Entity<GameParticipation>()
                .HasOne(gp => gp.Team)
                .WithMany(t => t.GameParticipations)
                .HasForeignKey(gp => gp.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JudgeScore>()
                .HasOne(js => js.Team)
                .WithMany(t => t.JudgeScores)
                .HasForeignKey(js => js.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Team)
                .WithMany(t => t.ServiceRequests)
                .HasForeignKey(sr => sr.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Team)
                .WithMany(t => t.Students)
                .HasForeignKey(s => s.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.EventDate })
                .IsUnique();
        }
    }
}
