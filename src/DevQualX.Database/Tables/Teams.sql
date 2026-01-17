CREATE TABLE [dbo].[Teams]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [GitHubTeamId] BIGINT NOT NULL UNIQUE,
    [InstallationId] INT NOT NULL,
    [Slug] NVARCHAR(100) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [SyncedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Teams] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Teams_Installations] FOREIGN KEY ([InstallationId]) REFERENCES [Installations]([Id]) ON DELETE CASCADE,
    INDEX [IX_Teams_GitHubTeamId] NONCLUSTERED ([GitHubTeamId] ASC),
    INDEX [IX_Teams_InstallationId_Slug] NONCLUSTERED ([InstallationId], [Slug] ASC)
);
