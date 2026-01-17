CREATE TABLE [dbo].[RoleAssignments]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [InstallationId] INT NOT NULL,
    [UserId] INT NULL,
    [TeamId] INT NULL,
    [Role] NVARCHAR(50) NOT NULL, -- Owner, Admin, Maintainer, Reader
    [Scope] NVARCHAR(50) NOT NULL, -- Organization, Repository, GitHubProject
    [ResourceId] INT NULL, -- FK to Repositories.Id or GitHubProjects.Id (NULL = org-wide)
    [GrantedBy] INT NOT NULL,
    [GrantedAt] DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_RoleAssignments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_RoleAssignments_Installations] FOREIGN KEY ([InstallationId]) REFERENCES [Installations]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RoleAssignments_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]),
    CONSTRAINT [FK_RoleAssignments_Teams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([Id]),
    CONSTRAINT [FK_RoleAssignments_GrantedBy] FOREIGN KEY ([GrantedBy]) REFERENCES [Users]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [CK_RoleAssignments_UserOrTeam] CHECK (([UserId] IS NOT NULL AND [TeamId] IS NULL) OR ([UserId] IS NULL AND [TeamId] IS NOT NULL)),
    CONSTRAINT [CK_RoleAssignments_Role] CHECK ([Role] IN ('Owner', 'Admin', 'Maintainer', 'Reader')),
    CONSTRAINT [CK_RoleAssignments_Scope] CHECK ([Scope] IN ('Organization', 'Repository', 'GitHubProject')),
    INDEX [IX_RoleAssignments_Installation_User] NONCLUSTERED ([InstallationId], [UserId] ASC),
    INDEX [IX_RoleAssignments_Installation_Team] NONCLUSTERED ([InstallationId], [TeamId] ASC),
    INDEX [IX_RoleAssignments_Scope_Resource] NONCLUSTERED ([Scope], [ResourceId] ASC)
);
