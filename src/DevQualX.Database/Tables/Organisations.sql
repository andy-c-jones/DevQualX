CREATE TABLE [dbo].[Organisations]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [Name] NVARCHAR(200) NOT NULL,
    CONSTRAINT [PK_Organisations] PRIMARY KEY CLUSTERED ([Id] ASC)
);
