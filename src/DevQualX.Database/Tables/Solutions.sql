CREATE TABLE [dbo].[Solutions]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [RepositoryId] INT NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [RelativePath] NVARCHAR(500) NOT NULL, -- Path within repo (e.g., src/MySolution.sln)
    [DiscoveredAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [LastReportAt] DATETIMEOFFSET NULL,
    CONSTRAINT [PK_Solutions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Solutions_Repositories] FOREIGN KEY ([RepositoryId]) REFERENCES [Repositories]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_Solutions_Repo_Path] UNIQUE ([RepositoryId], [RelativePath]),
    INDEX [IX_Solutions_RepositoryId] NONCLUSTERED ([RepositoryId] ASC)
);
