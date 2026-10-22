using Microsoft.EntityFrameworkCore;
using NasaSpaceApps.Domain.Entities;
using NasaSpaceApps.Infrastructure.Services;

namespace NasaSpaceApps.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(NasaSpaceAppsDbContext context)
    {
        // InMemory provider: EF creates the schema from the model automatically.
        // SQL Server: create tables via IF NOT EXISTS raw SQL — never call
        // EnsureCreated()/EnsureDeleted() because shared hosts (e.g. ASPMonster)
        // often lack CREATE DATABASE / DROP DATABASE permission.
        if (!context.Database.IsRelational())
        {
            context.Database.EnsureCreated();
        }
        else
        {
            CreateTablesIfNotExists(context);
        }

        try
        {
            if (context.Challenges.Any())
            {
                return;
            }
        }
        catch
        {
            // Database unreachable or table missing — skip seeding
            return;
        }

        Seed(context);
    }

    private static void Seed(NasaSpaceAppsDbContext context)
    {
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
            new JudgingCriterion { Challenge = challenge, Name = "Team Work", MaxScore = 10m, Weight = 30m, Description = "How well the team collaborated and delivered together." },
            new JudgingCriterion { Challenge = challenge, Name = "Challenge Relevance", MaxScore = 10m, Weight = 40m, Description = "How directly the project addresses the challenge brief." },
            new JudgingCriterion { Challenge = challenge, Name = "Technologies Used", MaxScore = 10m, Weight = 30m, Description = "Quality and appropriateness of the technologies behind the solution." }
        };

        context.JudgingCriteria.AddRange(criteria);

        context.CriterionQuestions.AddRange(
            new CriterionQuestion { Criterion = criteria[0], Text = "Did every team member contribute, and were the roles clear?", DisplayOrder = 1 },
            new CriterionQuestion { Criterion = criteria[0], Text = "How well did the team communicate, document, and hand over work?", DisplayOrder = 2 },
            new CriterionQuestion { Criterion = criteria[1], Text = "Does the project clearly address the challenge brief and its target users?", DisplayOrder = 1 },
            new CriterionQuestion { Criterion = criteria[1], Text = "Is the solution grounded in the data and methods the challenge requires?", DisplayOrder = 2 },
            new CriterionQuestion { Criterion = criteria[2], Text = "Are the technologies used appropriate, current, and well justified for the problem?", DisplayOrder = 1 },
            new CriterionQuestion { Criterion = criteria[2], Text = "Is the implementation solid, reproducible, and of good engineering quality?", DisplayOrder = 2 });

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

        context.SeasonMedia.AddRange(
            new SeasonMedia
            {
                Season = "2025",
                Title = "Global Hackathon Highlights",
                Description = "Footage from the 2025 season.",
                Link = "https://example.com/2025-highlights",
                MediaType = "video",
                DisplayOrder = 1,
                Active = true
            },
            new SeasonMedia
            {
                Season = "2024",
                Title = "Hurghada Local Event",
                Description = "Photos from the 2024 local event.",
                Link = "https://example.com/2024-gallery",
                MediaType = "image",
                DisplayOrder = 2,
                Active = true
            });

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
            MemberType = CoreTeamTypes.Organizer,
            Status = "Active"
        };

        var mentor1 = new Staff
        {
            FullName = "Sara Hassan",
            Username = "sara.mentor",
            Email = "sara.mentor@nasa.local",
            PasswordHash = passwordHasher.Hash("Mentor123!"),
            Role = AppRoles.CoreTeam,
            MemberType = CoreTeamTypes.Mentor,
            Status = "Active"
        };

        var dev1 = new Staff
        {
            FullName = "Omar Khaled",
            Username = "omar.dev",
            Email = "omar.dev@nasa.local",
            PasswordHash = passwordHasher.Hash("Dev123!"),
            Role = AppRoles.CoreTeam,
            MemberType = CoreTeamTypes.Developer,
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

        context.Staff.AddRange(admin, coreTeam, mentor1, dev1, judgeStaff);

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

        var criterionQuestions = context.CriterionQuestions.OrderBy(q => q.CriterionQuestionId).ToList();

        context.QuestionScores.AddRange(
            new QuestionScore { Team = team1, Criterion = criteria[0], CriterionQuestion = criterionQuestions[0], Judge = judge, MaxScore = 5m, Score = 4.5m, Comment = "Very original." },
            new QuestionScore { Team = team1, Criterion = criteria[0], CriterionQuestion = criterionQuestions[1], Judge = judge, MaxScore = 5m, Score = 5m },
            new QuestionScore { Team = team1, Criterion = criteria[1], CriterionQuestion = criterionQuestions[2], Judge = judge, MaxScore = 5m, Score = 4m },
            new QuestionScore { Team = team1, Criterion = criteria[1], CriterionQuestion = criterionQuestions[3], Judge = judge, MaxScore = 5m, Score = 4.5m },
            new QuestionScore { Team = team2, Criterion = criteria[0], CriterionQuestion = criterionQuestions[0], Judge = judge, MaxScore = 5m, Score = 4m },
            new QuestionScore { Team = team2, Criterion = criteria[0], CriterionQuestion = criterionQuestions[1], Judge = judge, MaxScore = 5m, Score = 5m },
            new QuestionScore { Team = team2, Criterion = criteria[1], CriterionQuestion = criterionQuestions[2], Judge = judge, MaxScore = 5m, Score = 3m },
            new QuestionScore { Team = team2, Criterion = criteria[1], CriterionQuestion = criterionQuestions[3], Judge = judge, MaxScore = 5m, Score = 4m });
        context.SaveChanges();

        // The seeded judge has published both criteria they scored, so the demo
        // scoreboard is populated on first run.
        var publishedAt = DateTime.UtcNow;
        context.CriterionPublications.AddRange(
            new CriterionPublication { Criterion = criteria[0], Judge = judge, PublishedAt = publishedAt },
            new CriterionPublication { Criterion = criteria[1], Judge = judge, PublishedAt = publishedAt });

        criteria[0].IsPublished = true;
        criteria[0].PublishedAt = publishedAt;
        criteria[1].IsPublished = true;
        criteria[1].PublishedAt = publishedAt;
        context.SaveChanges();
    }

    /// <summary>
    /// Creates every table, index, and new column via IF NOT EXISTS raw SQL.
    /// Runs inside the existing database — never attempts CREATE/DROP DATABASE.
    /// </summary>
    private static void CreateTablesIfNotExists(NasaSpaceAppsDbContext context)
    {
        try
        {
            context.Database.ExecuteSqlRaw(SqlSchemaScript);
        }
        catch
        {
            // Tables already exist, or database is unreachable — non-fatal
        }
    }

    private const string SqlSchemaScript = @"
-- ============ Challenges ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Challenges')
BEGIN
    CREATE TABLE [Challenges] (
        [ChallengeId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Source] NVARCHAR(200) NULL,
        [Season] NVARCHAR(50) NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        [Image] NVARCHAR(MAX) NULL
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Challenges') AND name = 'Image')
        ALTER TABLE [Challenges] ADD [Image] NVARCHAR(MAX) NULL;
END

-- ============ Teams ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Teams')
BEGIN
    CREATE TABLE [Teams] (
        [TeamId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Username] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [ChallengeId] INT NOT NULL,
        [Logo] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
        CONSTRAINT [FK_Teams_Challenges] FOREIGN KEY ([ChallengeId]) REFERENCES [Challenges]([ChallengeId]) ON DELETE CASCADE
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Teams') AND name = 'Logo')
        ALTER TABLE [Teams] ADD [Logo] NVARCHAR(MAX) NULL;
END
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teams_Username' AND object_id = OBJECT_ID('Teams'))
CREATE UNIQUE INDEX [IX_Teams_Username] ON [Teams]([Username]);

-- ============ Judges ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Judges')
CREATE TABLE [Judges] (
    [JudgeId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Account] NVARCHAR(200) NULL,
    [Role] NVARCHAR(100) NULL
);

-- ============ Staff ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Staff')
BEGIN
    CREATE TABLE [Staff] (
        [StaffId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FullName] NVARCHAR(200) NOT NULL,
        [Username] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(200) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL,
        [MemberType] NVARCHAR(50) NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
        [JudgeId] INT NULL,
        [Photo] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_Staff_Judges] FOREIGN KEY ([JudgeId]) REFERENCES [Judges]([JudgeId]) ON DELETE SET NULL
    );
    CREATE UNIQUE INDEX [IX_Staff_Username] ON [Staff]([Username]);
    CREATE UNIQUE INDEX [IX_Staff_Email] ON [Staff]([Email]);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Staff') AND name = 'MemberType')
        ALTER TABLE [Staff] ADD [MemberType] NVARCHAR(50) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Staff') AND name = 'Photo')
        ALTER TABLE [Staff] ADD [Photo] NVARCHAR(MAX) NULL;
END

-- ============ Students ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
CREATE TABLE [Students] (
    [StudentId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeamId] INT NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Email] NVARCHAR(200) NULL,
    [Contact] NVARCHAR(100) NULL,
    [Status] NVARCHAR(50) NULL,
    CONSTRAINT [FK_Students_Teams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([TeamId]) ON DELETE CASCADE
);

-- ============ Projects ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Projects')
CREATE TABLE [Projects] (
    [ProjectId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeamId] INT NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [RepositoryLink] NVARCHAR(500) NULL,
    [DemoLink] NVARCHAR(500) NULL,
    CONSTRAINT [FK_Projects_Teams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([TeamId]) ON DELETE CASCADE
);

-- ============ JudgingCriteria ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'JudgingCriteria')
BEGIN
    CREATE TABLE [JudgingCriteria] (
        [CriterionId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ChallengeId] INT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [MaxScore] DECIMAL(18,2) NOT NULL,
        [Weight] DECIMAL(18,2) NOT NULL,
        [IsPublished] BIT NOT NULL DEFAULT 0,
        [PublishedAt] DATETIME2 NULL,
        CONSTRAINT [FK_JudgingCriteria_Challenges] FOREIGN KEY ([ChallengeId]) REFERENCES [Challenges]([ChallengeId]) ON DELETE SET NULL
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('JudgingCriteria') AND name = 'ChallengeId')
        ALTER TABLE [JudgingCriteria] ADD [ChallengeId] INT NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('JudgingCriteria') AND name = 'IsPublished')
        ALTER TABLE [JudgingCriteria] ADD [IsPublished] BIT NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('JudgingCriteria') AND name = 'PublishedAt')
        ALTER TABLE [JudgingCriteria] ADD [PublishedAt] DATETIME2 NULL;
END

-- ============ CriterionQuestions (article/reference prompts) ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CriterionQuestions')
CREATE TABLE [CriterionQuestions] (
    [CriterionQuestionId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CriterionId] INT NOT NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [FK_CriterionQuestions_Criteria] FOREIGN KEY ([CriterionId]) REFERENCES [JudgingCriteria]([CriterionId]) ON DELETE CASCADE
);

-- ============ QuestionScores (per-question + ad-hoc + skip) ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuestionScores')
CREATE TABLE [QuestionScores] (
    [QuestionScoreId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeamId] INT NOT NULL,
    [CriterionId] INT NOT NULL,
    [CriterionQuestionId] INT NULL,
    [QuestionName] NVARCHAR(500) NULL,
    [JudgeId] INT NOT NULL,
    [MaxScore] DECIMAL(18,2) NOT NULL,
    [Score] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Skipped] BIT NOT NULL DEFAULT 0,
    [Comment] NVARCHAR(MAX) NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_QuestionScores_Teams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([TeamId]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuestionScores_Criteria] FOREIGN KEY ([CriterionId]) REFERENCES [JudgingCriteria]([CriterionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuestionScores_CriterionQuestions] FOREIGN KEY ([CriterionQuestionId]) REFERENCES [CriterionQuestions]([CriterionQuestionId]) ON DELETE SET NULL,
    CONSTRAINT [FK_QuestionScores_Judges] FOREIGN KEY ([JudgeId]) REFERENCES [Judges]([JudgeId]) ON DELETE CASCADE
);

-- ============ CriterionPublications (per-judge publish of a criterion) ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CriterionPublications')
BEGIN
    CREATE TABLE [CriterionPublications] (
        [CriterionPublicationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CriterionId] INT NOT NULL,
        [JudgeId] INT NOT NULL,
        [PublishedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_CriterionPublications_Criteria] FOREIGN KEY ([CriterionId]) REFERENCES [JudgingCriteria]([CriterionId]) ON DELETE CASCADE,
        CONSTRAINT [FK_CriterionPublications_Judges] FOREIGN KEY ([JudgeId]) REFERENCES [Judges]([JudgeId]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_CriterionPublications_CriterionId_JudgeId] ON [CriterionPublications]([CriterionId], [JudgeId]);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CriterionPublications_CriterionId_JudgeId' AND object_id = OBJECT_ID('CriterionPublications'))
    CREATE UNIQUE INDEX [IX_CriterionPublications_CriterionId_JudgeId] ON [CriterionPublications]([CriterionId], [JudgeId]);
END

-- Backfill: criteria that were published globally before the per-judge model keep
-- their results on the board by publishing every judge who scored them.
INSERT INTO [CriterionPublications] ([CriterionId], [JudgeId], [PublishedAt])
SELECT DISTINCT qs.[CriterionId], qs.[JudgeId], GETUTCDATE()
FROM [QuestionScores] qs
INNER JOIN [JudgingCriteria] c ON c.[CriterionId] = qs.[CriterionId]
WHERE c.[IsPublished] = 1
  AND NOT EXISTS (
      SELECT 1 FROM [CriterionPublications] p
      WHERE p.[CriterionId] = qs.[CriterionId] AND p.[JudgeId] = qs.[JudgeId]
  );

-- ============ JudgeScores ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'JudgeScores')
CREATE TABLE [JudgeScores] (
    [ScoreId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeamId] INT NOT NULL,
    [JudgeId] INT NOT NULL,
    [CriterionId] INT NOT NULL,
    [Score] DECIMAL(18,2) NOT NULL,
    [Comment] NVARCHAR(MAX) NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_JudgeScores_Teams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([TeamId]) ON DELETE CASCADE,
    CONSTRAINT [FK_JudgeScores_Judges] FOREIGN KEY ([JudgeId]) REFERENCES [Judges]([JudgeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_JudgeScores_Criteria] FOREIGN KEY ([CriterionId]) REFERENCES [JudgingCriteria]([CriterionId]) ON DELETE CASCADE
);

-- ============ Attendances ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attendances')
BEGIN
    CREATE TABLE [Attendances] (
        [AttendanceId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [StudentId] INT NOT NULL,
        [EventDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [MarkedBy] NVARCHAR(200) NULL,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE UNIQUE INDEX [IX_Attendances_StudentId_EventDate] ON [Attendances]([StudentId], [EventDate]);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Attendances_StudentId_EventDate' AND object_id = OBJECT_ID('Attendances'))
    CREATE UNIQUE INDEX [IX_Attendances_StudentId_EventDate] ON [Attendances]([StudentId], [EventDate]);
END

-- ============ ServiceRequests ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceRequests')
CREATE TABLE [ServiceRequests] (
    [RequestId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeamId] INT NOT NULL,
    [Type] NVARCHAR(100) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ResolvedAt] DATETIME2 NULL,
    CONSTRAINT [FK_ServiceRequests_Teams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([TeamId]) ON DELETE CASCADE
);

-- ============ Questions ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Questions')
BEGIN
    CREATE TABLE [Questions] (
        [QuestionId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ChallengeId] INT NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [Text] NVARCHAR(MAX) NOT NULL,
        [Options] NVARCHAR(MAX) NULL,
        [CorrectAnswer] NVARCHAR(MAX) NULL,
        [IsPublished] BIT NOT NULL DEFAULT 0,
        [PublishedAt] DATETIME2 NULL,
        [Points] INT NOT NULL DEFAULT 10,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_Questions_Challenges] FOREIGN KEY ([ChallengeId]) REFERENCES [Challenges]([ChallengeId]) ON DELETE CASCADE
    );
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Questions') AND name = 'IsPublished')
        ALTER TABLE [Questions] ADD [IsPublished] BIT NOT NULL DEFAULT 0;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Questions') AND name = 'PublishedAt')
        ALTER TABLE [Questions] ADD [PublishedAt] DATETIME2 NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Questions') AND name = 'Points')
        ALTER TABLE [Questions] ADD [Points] INT NOT NULL DEFAULT 10;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Questions') AND name = 'IsActive')
        ALTER TABLE [Questions] ADD [IsActive] BIT NOT NULL DEFAULT 1;
END

-- ============ GameParticipations ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GameParticipations')
BEGIN
    CREATE TABLE [GameParticipations] (
        [ParticipationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [QuestionId] INT NOT NULL,
        [TeamId] INT NOT NULL,
        [Answer] NVARCHAR(MAX) NULL,
        [Correct] BIT NOT NULL DEFAULT 0,
        [SubmittedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_GameParticipations_Questions] FOREIGN KEY ([QuestionId]) REFERENCES [Questions]([QuestionId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GameParticipations_Teams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([TeamId]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_GameParticipations_QuestionId_TeamId] ON [GameParticipations]([QuestionId], [TeamId]);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GameParticipations_QuestionId_TeamId' AND object_id = OBJECT_ID('GameParticipations'))
    CREATE UNIQUE INDEX [IX_GameParticipations_QuestionId_TeamId] ON [GameParticipations]([QuestionId], [TeamId]);
END

-- ============ SponsorTiers ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SponsorTiers')
CREATE TABLE [SponsorTiers] (
    [TierId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Priority] INT NULL,
    [BannerEnabled] BIT NOT NULL DEFAULT 0
);

-- ============ Sponsors ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sponsors')
CREATE TABLE [Sponsors] (
    [SponsorId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Logo] NVARCHAR(MAX) NULL,
    [Link] NVARCHAR(500) NULL,
    [TierId] INT NOT NULL,
    [Active] BIT NOT NULL DEFAULT 0,
    [DisplayOrder] INT NULL,
    CONSTRAINT [FK_Sponsors_Tiers] FOREIGN KEY ([TierId]) REFERENCES [SponsorTiers]([TierId]) ON DELETE CASCADE
);
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Sponsors') AND name = 'Logo' AND max_length <> -1)
    ALTER TABLE [Sponsors] ALTER COLUMN [Logo] NVARCHAR(MAX) NULL;

-- ============ SeasonMedia ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SeasonMedia')
CREATE TABLE [SeasonMedia] (
    [MediaId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Season] NVARCHAR(50) NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Image] NVARCHAR(MAX) NULL,
    [Link] NVARCHAR(500) NULL,
    [MediaType] NVARCHAR(50) NULL,
    [DisplayOrder] INT NULL,
    [Active] BIT NOT NULL DEFAULT 1
);
";
}
