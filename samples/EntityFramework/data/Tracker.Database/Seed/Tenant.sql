/* Table [dbo].[Tenant] data */
SET IDENTITY_INSERT [dbo].[Tenant] ON;
GO


MERGE INTO [dbo].[Tenant] AS t
USING
(
    VALUES
    (1000, N'Battlestar Galactica', N'Battlestar Galactica'),
    (1001, N'Cylons', N'Cylons')
)
AS s
([Id], [Name], [Description])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [Description])
    VALUES (s.[Id], s.[Name], s.[Description])
WHEN MATCHED THEN
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description]
OUTPUT $action as MergeAction;

SET IDENTITY_INSERT [dbo].[Tenant] OFF;
GO

