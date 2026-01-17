CREATE TABLE [dbo].[Installations]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [GitHubInstallationId] BIGINT NOT NULL UNIQUE,
    [GitHubAccountId] BIGINT NOT NULL,
    [AccountType] NVARCHAR(20) NOT NULL, -- 'Organization' or 'User'
    [AccountLogin] NVARCHAR(100) NOT NULL,
    [InstalledBy] INT NOT NULL,
    [InstalledAt] DATETIMEOFFSET NOT NULL,
    [UpdatedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [SuspendedAt] DATETIMEOFFSET NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Installations] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Installations_Users] FOREIGN KEY ([InstalledBy]) REFERENCES [Users]([Id]),
    CONSTRAINT [CK_Installations_AccountType] CHECK ([AccountType] IN ('Organization', 'User')),
    INDEX [IX_Installations_GitHubInstallationId] NONCLUSTERED ([GitHubInstallationId] ASC),
    INDEX [IX_Installations_AccountLogin] NONCLUSTERED ([AccountLogin] ASC),
    INDEX [IX_Installations_IsActive] NONCLUSTERED ([IsActive] ASC)
);
