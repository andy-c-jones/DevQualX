CREATE TABLE [dbo].[CSharpProjects]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [RepositoryId] INT NOT NULL,
    [SolutionId] INT NULL, -- NULL if standalone csproj
    [Name] NVARCHAR(200) NOT NULL,
    [RelativePath] NVARCHAR(500) NOT NULL, -- Path within repo (e.g., src/MyProject/MyProject.csproj)
    [TargetFramework] NVARCHAR(50) NULL,
    [DiscoveredAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [LastReportAt] DATETIMEOFFSET NULL,
    CONSTRAINT [PK_CSharpProjects] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CSharpProjects_Repositories] FOREIGN KEY ([RepositoryId]) REFERENCES [Repositories]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CSharpProjects_Solutions] FOREIGN KEY ([SolutionId]) REFERENCES [Solutions]([Id]),
    CONSTRAINT [UQ_CSharpProjects_Repo_Path] UNIQUE ([RepositoryId], [RelativePath]),
    INDEX [IX_CSharpProjects_RepositoryId] NONCLUSTERED ([RepositoryId] ASC),
    INDEX [IX_CSharpProjects_SolutionId] NONCLUSTERED ([SolutionId] ASC)
);
