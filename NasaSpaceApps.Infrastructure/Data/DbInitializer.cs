using Microsoft.EntityFrameworkCore;
using NasaSpaceApps.Domain.Entities;
using NasaSpaceApps.Infrastructure.Services;

namespace NasaSpaceApps.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(NasaSpaceAppsDbContext context)
    {
        // Handle schema evolution: if new tables/columns are missing (added after
        // the initial EnsureCreated), create them via raw SQL. Avoids
        // drop-and-recreate which requires elevated permissions on shared hosts.
        EnsureSchema(context);
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

        // Staff accounts: Admin, CoreTeam, Judge (one login pool for all non-participant roles)
        var admin = new Staff
        {
            FullName = "Event Admin",
            Username = "admin",
            Email = "admin@nasa.local",
            PasswordHash = passwordHasher.Hash("Admin123!"),
            Role = AppRoles.Admin,
            Status = "Active"
        };

        var coreTeam = new Staff
        {
            FullName = "Core Team Lead",
            Username = "coreteam",
            Email = "core@nasa.local",
            PasswordHash = passwordHasher.Hash("Core123!"),
            Role = AppRoles.CoreTeam,
            Status = "Active"
        };

        var judgeStaff = new Staff
        {
            FullName = "Dr. Ada Smith",
            Username = "judge",
            Email = "judge@nasa.local",
            PasswordHash = passwordHasher.Hash("Judge123!"),
            Role = AppRoles.Judge,
            Status = "Active",
            Judge = judge
        };

        context.Staff.AddRange(admin, coreTeam, judgeStaff);

        // Sample questions for the side trophy game
        var question1 = new Question
        {
            Challenge = challenge,
            Type = "multiple-choice",
            Text = "What does NASA stand for?",
            Options = "[\"National Aeronautics and Space Administration\",\"North American Space Association\",\"National Aerospace Science Agency\",\"Nuclear Aeronautics and Space Administration\"]",
            CorrectAnswer = "National Aeronautics and Space Administration",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow,
            Points = 10,
            IsActive = true
        };

        var question2 = new Question
        {
            Challenge = challenge,
            Type = "multiple-choice",
            Text = "Which planet is known as the Red Planet?",
            Options = "[\"Venus\",\"Mars\",\"Jupiter\",\"Saturn\"]",
            CorrectAnswer = "Mars",
            IsPublished = false,
            Points = 10,
            IsActive = true
        };

        context.Questions.AddRange(question1, question2);

        context.SaveChanges();

        var team1Score1 = new JudgeScore { TeamId = team1.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[0].CriterionId, Score = 9.5m, Comment = "Strong concept" };
        var team1Score2 = new JudgeScore { TeamId = team1.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[1].CriterionId, Score = 8.5m, Comment = "Solid execution" };
        var team2Score1 = new JudgeScore { TeamId = team2.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[0].CriterionId, Score = 9m, Comment = "Good idea" };
        var team2Score2 = new JudgeScore { TeamId = team2.TeamId, JudgeId = judge.JudgeId, CriterionId = criteria[1].CriterionId, Score = 7m, Comment = "Needs polish" };

        context.JudgeScores.AddRange(team1Score1, team1Score2, team2Score1, team2Score2);
        context.SaveChanges();
    }

    /// <summary>
    /// Creates any tables/columns that were added after the initial EnsureCreated.
    /// Uses raw SQL so it works on shared SQL Server hosts without CREATE DATABASE
    /// permission. No-ops on the InMemory provider.
    /// </summary>
    private static void EnsureSchema(NasaSpaceAppsDbContext context)
    {
        // Only applicable to relational (SQL Server) providers
        if (!context.Database.IsRelational())
        {
            return;
        }

        var conn = context.Database.GetDbConnection();

        try
        {
            conn.Open();
        }
        catch
        {
            // InMemory or connection not ready yet — EnsureCreated will handle it
            return;
        }

        using var command = conn.CreateCommand();

        // --- Staff table ---
        command.CommandText = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Staff')
BEGIN
    CREATE TABLE [Staff] (
        [StaffId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FullName] NVARCHAR(200) NOT NULL,
        [Username] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(200) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
        [JudgeId] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_Staff_Judges] FOREIGN KEY ([JudgeId]) REFERENCES [Judges]([JudgeId]) ON DELETE SET NULL
    );
    CREATE UNIQUE INDEX [IX_Staff_Username] ON [Staff]([Username]);
    CREATE UNIQUE INDEX [IX_Staff_Email] ON [Staff]([Email]);
END";

        try
        {
            command.ExecuteNonQuery();
        }
        catch
        {
            // InMemory provider or non-SQL — skip
            return;
        }

        // --- Question: new columns for publishing / side-trophy ---
        AddColumnIfMissing(context, "Questions", "IsPublished", "BIT NOT NULL DEFAULT 0");
        AddColumnIfMissing(context, "Questions", "PublishedAt", "DATETIME2 NULL");
        AddColumnIfMissing(context, "Questions", "Points", "INT NOT NULL DEFAULT 10");
        AddColumnIfMissing(context, "Questions", "IsActive", "BIT NOT NULL DEFAULT 1");

        // --- Attendance: unique index for upsert semantics ---
        try
        {
            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Attendances_StudentId_EventDate' AND object_id = OBJECT_ID('Attendances'))
    CREATE UNIQUE INDEX [IX_Attendances_StudentId_EventDate] ON [Attendances]([StudentId], [EventDate]);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GameParticipations_QuestionId_TeamId' AND object_id = OBJECT_ID('GameParticipations'))
    CREATE UNIQUE INDEX [IX_GameParticipations_QuestionId_TeamId] ON [GameParticipations]([QuestionId], [TeamId]);";
            cmd2.ExecuteNonQuery();
        }
        catch
        {
            // ignore — indexes are non-critical
        }
    }

    private static void AddColumnIfMissing(NasaSpaceAppsDbContext context, string table, string column, string definition)
    {
        try
        {
            using var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = $@"
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('{table}') AND name = '{column}'
)
    ALTER TABLE [{table}] ADD [{column}] {definition}";
            cmd.ExecuteNonQuery();
        }
        catch
        {
            // ignore
        }
    }
}
