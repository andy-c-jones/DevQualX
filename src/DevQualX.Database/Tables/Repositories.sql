CREATE TABLE [dbo].[Repositories]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [GitHubRepositoryId] BIGINT NOT NULL UNIQUE,
    [InstallationId] INT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [FullName] NVARCHAR(200) NOT NULL, -- org/repo format
    [IsPrivate] BIT NOT NULL DEFAULT 1,
    [DefaultBranch] NVARCHAR(100) NULL DEFAULT 'main',
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_Repositories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Repositories_Installations] FOREIGN KEY ([InstallationId]) REFERENCES [Installations]([Id]) ON DELETE CASCADE,
    INDEX [IX_Repositories_GitHubRepositoryId] NONCLUSTERED ([GitHubRepositoryId] ASC),
    INDEX [IX_Repositories_InstallationId] NONCLUSTERED ([InstallationId] ASC),
    INDEX [IX_Repositories_FullName] NONCLUSTERED ([FullName] ASC),
    INDEX [IX_Repositories_IsActive] NONCLUSTERED ([IsActive] ASC)
);
