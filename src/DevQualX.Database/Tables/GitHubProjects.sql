CREATE TABLE [dbo].[GitHubProjects]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [GitHubProjectId] BIGINT NOT NULL UNIQUE,
    [InstallationId] INT NOT NULL,
    [Title] NVARCHAR(300) NOT NULL,
    [Number] INT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [SyncedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_GitHubProjects] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_GitHubProjects_Installations] FOREIGN KEY ([InstallationId]) REFERENCES [Installations]([Id]) ON DELETE CASCADE,
    INDEX [IX_GitHubProjects_GitHubProjectId] NONCLUSTERED ([GitHubProjectId] ASC),
    INDEX [IX_GitHubProjects_InstallationId] NONCLUSTERED ([InstallationId] ASC)
);
