CREATE TABLE [Challenge] (
  [ChallengeId] int PRIMARY KEY IDENTITY(1, 1),
  [Title] varchar(200) NOT NULL,
  [Description] text,
  [Source] varchar(200),
  [Season] varchar(50),
  [Metadata] text
)
GO

CREATE TABLE [Team] (
  [TeamId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] varchar(150) NOT NULL,
  [Username] varchar(100) UNIQUE NOT NULL,
  [PasswordHash] varchar(255) NOT NULL,
  [ChallengeId] int NOT NULL,
  [Status] varchar(50) NOT NULL
)
GO

CREATE TABLE [Student] (
  [StudentId] int PRIMARY KEY IDENTITY(1, 1),
  [TeamId] int NOT NULL,
  [Name] varchar(150) NOT NULL,
  [Email] varchar(255),
  [Contact] varchar(100),
  [Status] varchar(50)
)
GO

CREATE TABLE [Project] (
  [ProjectId] int PRIMARY KEY IDENTITY(1, 1),
  [TeamId] int UNIQUE NOT NULL,
  [Title] varchar(200) NOT NULL,
  [Description] text,
  [RepositoryLink] varchar(500),
  [DemoLink] varchar(500)
)
GO

CREATE TABLE [Judge] (
  [JudgeId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] varchar(150) NOT NULL,
  [Account] varchar(255),
  [Role] varchar(100)
)
GO

CREATE TABLE [JudgingCriterion] (
  [CriterionId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] varchar(150) NOT NULL,
  [Description] text,
  [MaxScore] decimal(10,2) NOT NULL,
  [Weight] decimal(5,2)
)
GO

CREATE TABLE [JudgeScore] (
  [ScoreId] int PRIMARY KEY IDENTITY(1, 1),
  [TeamId] int NOT NULL,
  [JudgeId] int NOT NULL,
  [CriterionId] int NOT NULL,
  [Score] decimal(10,2) NOT NULL,
  [Comment] text,
  [Timestamp] timestamp NOT NULL
)
GO

CREATE TABLE [Attendance] (
  [AttendanceId] int PRIMARY KEY IDENTITY(1, 1),
  [StudentId] int NOT NULL,
  [EventDate] date NOT NULL,
  [Status] varchar(20) NOT NULL,
  [MarkedBy] varchar(150),
  [Timestamp] timestamp NOT NULL
)
GO

CREATE TABLE [ServiceRequest] (
  [RequestId] int PRIMARY KEY IDENTITY(1, 1),
  [TeamId] int NOT NULL,
  [Type] varchar(100) NOT NULL,
  [Message] text NOT NULL,
  [Status] varchar(50) NOT NULL,
  [CreatedAt] timestamp NOT NULL,
  [ResolvedAt] timestamp
)
GO

CREATE TABLE [Question] (
  [QuestionId] int PRIMARY KEY IDENTITY(1, 1),
  [ChallengeId] int NOT NULL,
  [Type] varchar(50) NOT NULL,
  [Text] text NOT NULL,
  [Options] text,
  [CorrectAnswer] text
)
GO

CREATE TABLE [GameParticipation] (
  [ParticipationId] int PRIMARY KEY IDENTITY(1, 1),
  [QuestionId] int NOT NULL,
  [TeamId] int NOT NULL,
  [Answer] text,
  [Correct] boolean,
  [SubmittedAt] timestamp NOT NULL
)
GO

CREATE TABLE [SponsorTier] (
  [TierId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] varchar(100) NOT NULL,
  [Priority] int,
  [BannerEnabled] boolean NOT NULL
)
GO

CREATE TABLE [Sponsor] (
  [SponsorId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] varchar(200) NOT NULL,
  [Logo] varchar(500),
  [Link] varchar(500),
  [TierId] int NOT NULL,
  [Active] boolean NOT NULL,
  [DisplayOrder] int
)
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'NASA challenge catalog entry. One Challenge can be assigned to many Teams and can contain many Questions.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Challenge';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'One shared login/account per team. Each Team is associated with exactly one Challenge.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Team';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Every Student belongs to exactly one Team.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Student';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'A Team can have zero or one Project. Each Project belongs to exactly one Team.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Project';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Represents a judge who evaluates teams.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Judge';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Configurable scoring criterion.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'JudgingCriterion';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Stores a traceable score given by a Judge to a Team for a specific JudgingCriterion.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'JudgeScore';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Attendance status examples: Present, Late, Absent.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Attendance';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Teams can submit multiple service requests to organizers or mentors.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ServiceRequest';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Questions can be used for evaluation/judging or prize games.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Question';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Tracks a Team answer to a Question in the prize game.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'GameParticipation';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Defines sponsorship tiers and controls sponsor visibility and banner behavior.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'SponsorTier';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = 'Each Sponsor belongs to exactly one SponsorTier.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Sponsor';
GO

ALTER TABLE [Team] ADD FOREIGN KEY ([ChallengeId]) REFERENCES [Challenge] ([ChallengeId])
GO

ALTER TABLE [Student] ADD FOREIGN KEY ([TeamId]) REFERENCES [Team] ([TeamId])
GO

ALTER TABLE [Project] ADD FOREIGN KEY ([TeamId]) REFERENCES [Team] ([TeamId])
GO

ALTER TABLE [JudgeScore] ADD FOREIGN KEY ([TeamId]) REFERENCES [Team] ([TeamId])
GO

ALTER TABLE [JudgeScore] ADD FOREIGN KEY ([JudgeId]) REFERENCES [Judge] ([JudgeId])
GO

ALTER TABLE [JudgeScore] ADD FOREIGN KEY ([CriterionId]) REFERENCES [JudgingCriterion] ([CriterionId])
GO

ALTER TABLE [Attendance] ADD FOREIGN KEY ([StudentId]) REFERENCES [Student] ([StudentId])
GO

ALTER TABLE [ServiceRequest] ADD FOREIGN KEY ([TeamId]) REFERENCES [Team] ([TeamId])
GO

ALTER TABLE [Question] ADD FOREIGN KEY ([ChallengeId]) REFERENCES [Challenge] ([ChallengeId])
GO

ALTER TABLE [GameParticipation] ADD FOREIGN KEY ([QuestionId]) REFERENCES [Question] ([QuestionId])
GO

ALTER TABLE [GameParticipation] ADD FOREIGN KEY ([TeamId]) REFERENCES [Team] ([TeamId])
GO

ALTER TABLE [Sponsor] ADD FOREIGN KEY ([TierId]) REFERENCES [SponsorTier] ([TierId])
GO
